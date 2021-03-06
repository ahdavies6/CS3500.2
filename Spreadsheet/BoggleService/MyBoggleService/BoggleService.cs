﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using static System.Net.HttpStatusCode;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace Boggle
{
    /// <summary>
    /// Boggle game server that keeps track of user data, game data, and serves it up as necessary
    /// for a Boggle game client to receive. Also allows users to interact with the server as necessary
    /// to create "accounts" and play the game against other users.
    /// </summary>
    public class BoggleService
    {
        /// <summary>
        /// The connection string to the database that contains all of the server's data
        /// </summary>
        private static string BoggleDB;

        /// <summary>
        /// Dictionary of valid words
        /// </summary>
        private static HashSet<string> Dict;

        /// <summary>
        /// Saves the database's connection string and the dictionary's filepath
        /// </summary>
        static BoggleService()
        {
            string dbFolder = System.IO.Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string connectionString = String.Format(@"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = {0}\BoggleDB.mdf; Integrated Security = True", dbFolder);
            BoggleDB = connectionString;

            Dict = new HashSet<string>();

            using (StreamReader file = new StreamReader("dictionary.txt"))
            {
                string currLine;
                while ((currLine = file.ReadLine()) != null)
                {
                    Dict.Add(currLine.ToLower());
                }
            };
        }


        /// <summary>
        /// Creates a user with nickname. 
        /// 
        /// If nickname is null, or is empty when trimmed, responds with status 403 (Forbidden). 
        /// 
        /// Otherwise, creates a new user with a unique UserToken and the trimmed nickname.
        /// The returned UserToken should be used to identify the user in subsequent requests.
        /// Responds with status 201 (Created). 
        /// </summary>
        public UserTokenResponse RegisterUser(CreateUserRequest request, out HttpStatusCode status)
        {
            //null checks
            if (request is null || request.Nickname is null || (request.Nickname = request.Nickname.Trim()).Length == 0)
            {
                status = Forbidden;
                return null;
            }

            //Insert the new user into 
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand("insert into Users(UserID, Nickname) values(@UserID, @Nickname)", conn, trans))
                    {
                        string uid = Guid.NewGuid().ToString();

                        cmd.Parameters.AddWithValue("@UserID", uid);
                        cmd.Parameters.AddWithValue("@Nickname", request.Nickname);

                        //try to do the insert 
                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }

                        //Commit
                        trans.Commit();

                        //Returning values
                        status = Created;
                        return new UserTokenResponse()
                        {
                            UserToken = uid
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to join a game with user userToken and timeLimit
        /// 
        /// If UserToken is invalid, TimeLimit is less than 5, or TimeLimit is over 120, responds
        /// with status 403 (Forbidden).
        /// 
        /// Otherwise, if UserToken is already a player in the pending game, responds with status
        /// 409 (Conflict).
        /// 
        /// Otherwise, if there is already one player in the pending game, adds UserToken as the
        /// second player. The pending game becomes active. The active game's time limit is the integer
        /// average of the time limits requested by the two players. Returns the new active game's
        /// GameID (which should be the same as the old pending game's GameID). Responds with
        /// status 201 (Created).
        /// 
        /// Otherwise, adds UserToken as the first player of a new pending game, and the TimeLimit as
        /// the pending game's requested time limit. Returns the pending game's GameID. Responds with
        /// status 202 (Accepted).
        /// </summary>
        public GameIDResponse JoinGame(JoinRequest request, out HttpStatusCode status)
        {
            int timeLimit = request.TimeLimit;

            // make sure TimeLimit is within the expected bounds
            if (!(timeLimit >= 5 && timeLimit <= 120))
            {
                status = Forbidden;
                return null;
            }

            //null checks 
            if (request is null || request.UserToken is null)
            {
                status = Forbidden;
                return null;
            }

            // the UserID that'll be used throughout this method
            string userID = request.UserToken.Trim();

            // make sure UserToken is within the expected bounds
            if (userID == null || userID.Length == 0 || userID.Length > 36)
            {
                status = Forbidden;
                return null;
            }

            // open connection to database
            using (SqlConnection connection = new SqlConnection(BoggleDB))
            {
                connection.Open();

                // execute all commands within a single transaction
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    // todo: make sure GameID starts at 1, not 0
                    // (otherwise, this should be changed, because this might actually end up as 0 when creating a game)
                    int gameID = -1;

                    // find out whether there's a pending game
                    using (SqlCommand command = new SqlCommand("select GameID, Player1, TimeLimit from Games where Player2 is null",
                        connection, transaction))
                    {
                        // the reader that will determine whether there's a pending game, and if so, what its gameID is
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                gameID = (int)reader["GameID"];
                                string player1 = (string)reader["Player1"];

                                // averages the time between player1's requested time and this user's requested time
                                int player1RequestedTime = (int)reader["TimeLimit"];
                                timeLimit = (timeLimit + player1RequestedTime) / 2;

                                // player is already searching for a game
                                if (player1 == userID)
                                {
                                    status = Conflict;
                                    reader.Close();
                                    transaction.Commit();
                                    return null;
                                }
                            }
                        }
                    }

                    if (gameID == -1)
                    {
                        using (SqlCommand command = new SqlCommand("insert into Games (Player1, TimeLimit) output inserted.GameID values(@Player1, @TimeLimit)", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Player1", userID);
                            command.Parameters.AddWithValue("@TimeLimit", timeLimit);

                            SqlParameter outputparam = new SqlParameter("@GameID", SqlDbType.Int)
                            {
                                Direction = ParameterDirection.Output
                            };

                            command.Parameters.Add(outputparam);

                            // execute command, and get back the primary key (GameID)
                            try
                            {
                                gameID = (int)command.ExecuteScalar();

                            }
                            //If an exception was thrown, a foreign key pair was violated
                            catch (SqlException)
                            {
                                status = Forbidden;
                                transaction.Commit();
                                return null;
                            }
                            status = Accepted;
                        }
                    }
                    else
                    {
                        using (SqlCommand command = new SqlCommand("update Games set Player2 = @Player2, Board = @Board, TimeLimit = @TimeLimit, StartTime = @StartTime where GameID = @GameID", connection, transaction))
                        {
                            command.Parameters.AddWithValue("@Player2", userID);
                            command.Parameters.AddWithValue("@Board", StaticBoard.GenerateBoggleBoard());
                            command.Parameters.AddWithValue("@TimeLimit", timeLimit);
                            command.Parameters.AddWithValue("@StartTime", DateTime.UtcNow);
                            command.Parameters.AddWithValue("@GameID", gameID);

                            try
                            {
                                if (command.ExecuteNonQuery() != 1)
                                {
                                    throw new Exception("SQL query failed");
                                }
                            }
                            //If Player2 is not a registered user
                            catch (SqlException)
                            {
                                status = Forbidden;
                                transaction.Commit();
                                return null;
                            }

                            status = Created;

                        }

                    }

                    transaction.Commit();
                    return new GameIDResponse { GameID = gameID };
                }
            }
        }

        /// <summary>
        /// Cancels an active join request from user userToken.
        /// 
        /// If UserToken is invalid or is not a player in the pending game, responds with status
        /// 403 (Forbidden).
        /// 
        /// Otherwise, removes UserToken from the pending game and responds with status 200 (OK).
        /// </summary>
        public void CancelJoinRequest(CancelJoinRequest request, out HttpStatusCode status)
        {

            if (request is null || request.UserToken is null || (request.UserToken = request.UserToken.Trim()).Length == 0)
            {
                status = Forbidden;
                return;
            }

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand("delete from Games where Player1 = @Player1Token and Player2 is null", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Player1Token", request.UserToken);

                        //Checks if any lines were deleted. If none were deleted then UserToken  was not in any pending games
                        if (cmd.ExecuteNonQuery() == 0)
                        {
                            status = Forbidden;
                        }
                        else
                        {
                            status = OK;
                        }

                        trans.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Plays word to game gameID
        /// 
        /// If Word is null or empty or longer than 30 characters when trimmed, or if GameID
        /// or UserToken is invalid, or if UserToken is not a player in the game identified by GameID,
        /// responds with response code 403 (Forbidden).
        /// 
        /// Otherwise, if the game state is anything other than "active", responds with response
        /// code 409 (Conflict).
        /// 
        /// Otherwise, records the trimmed Word as being played by UserToken in the game identified by
        /// GameID. Returns the score for Word in the context of the game (e.g. if Word has been played before
        /// the score is zero). Responds with status 200 (OK). Note: The word is not case sensitive.
        /// </summary>
        public ScoreResponse PlayWord(PlayWord request, string gameID, out HttpStatusCode status)
        {
            //null and length checks
            if (gameID is null || request is null || request.UserToken is null || (request.UserToken = request.UserToken.Trim()).Length == 0
                || request.Word is null || (request.Word = request.Word.Trim()).Length == 0 || request.Word.Length > 30)
            {
                status = Forbidden;
                return null;
            }

            //values used when doing the query then insert into words
            string player1, player2, board;
            int timeLimit;
            DateTime? startTime;

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand("select GameID, Player1, Player2, Board, TimeLimit, StartTime from Games where GameID = @GameID", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@GameID", gameID);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            //Game doesnt exist
                            if (!reader.HasRows)
                            {
                                status = Forbidden;
                                reader.Close();
                                //trans.Commit();
                                return null;
                            }

                            //only one row 
                            reader.Read();

                            //test if pending
                            if (DBNull.Value.Equals(reader["Player2"]))
                            {
                                status = Conflict;
                                reader.Close();
                                trans.Commit();
                                return null;
                            }

                            player1 = (string)reader["Player1"];
                            player2 = (string)reader["Player2"];

                            board = reader["Board"]?.ToString();
                            timeLimit = (int)reader["TimeLimit"];
                            startTime = reader.GetDateTime(5);
                        }
                    }


                    //Checks if the player is in the game
                    if (!(player1.Equals(request.UserToken) || player2.Equals(request.UserToken)))
                    {
                        status = Forbidden;
                        trans.Commit();
                        return null;
                    }

                    //check if the game is compeleted
                    if ((startTime?.AddSeconds(timeLimit) - DateTime.UtcNow)?.TotalMilliseconds < 0)
                    {
                        status = Conflict;
                        trans.Commit();
                        return null;
                    }

                    //Standardize words to lower case
                    request.Word = request.Word.ToLower();

                    int score = ScoreWord(board, request.Word);

                    //Check if the word has already been played by this player
                    using (SqlCommand cmd = new SqlCommand("select * from Words where Word = @Word and GameID = @GameID and Player = @Player", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Word", request.Word);
                        cmd.Parameters.AddWithValue("@GameID", gameID);
                        cmd.Parameters.AddWithValue("@Player", request.UserToken);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                score = 0;
                            }
                        }
                    }

                    //add the new word
                    using (SqlCommand cmd = new SqlCommand("insert into Words (Word, GameID, Player, Score) values(@Word, @GameID, @Player, @Score)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@Word", request.Word);
                        cmd.Parameters.AddWithValue("@GameID", gameID);
                        cmd.Parameters.AddWithValue("@Player", request.UserToken);
                        cmd.Parameters.AddWithValue("@Score", score);

                        if (cmd.ExecuteNonQuery() != 1)
                        {
                            throw new Exception("Query failed unexpectedly");
                        }

                        trans.Commit();

                        status = OK;
                        return new ScoreResponse() { Score = score };

                    }

                }
            }
        }

        /// <summary>
        /// Scores the passed word in the passed boggle board. It assumes the word has not been played before.
        /// 
        /// Case insensitive.
        /// </summary>
        private int ScoreWord(string boardString, string word)
        {
            //If the word is too small
            if (word.Length < 3)
            {
                return 0;
            }
            //Valid word scoring
            else if (StaticBoard.CanBeFormed(boardString, word) && Dict.Contains(word.ToLower()))
            {
                int leng = word.Length;

                if (leng == 3 || leng == 4)
                {
                    return 1;
                }
                else if (leng == 5 || leng == 6)
                {
                    return leng - 3;
                }
                else if (leng == 7)
                {
                    return 5;
                }
                else
                {
                    return 11;
                }
            }
            //Invalid word that is longer is length 3
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Returns the game status of game GameID.
        /// 
        /// If GameID is invalid, responds with status 403 (Forbidden).
        /// Otherwise, returns information about the game named by GameID as illustrated below. Note that
        /// the information returned depends on whether brief was included as a parameter as well as
        /// on the state of the game. Responds with status code 200 (OK). Note: The Board and Words are
        /// not case sensitive.
        /// </summary>
        public FullStatusResponse GetGameStatus(string gameID, string brief, out HttpStatusCode status)
        {
            bool briefbool;

            if (brief != null)
            {
                briefbool = brief.ToLower().Equals("yes");
            }
            else
            {
                briefbool = false;
            }

            //The db uses ints as the game keys, so try to make the gameID an int
            int id;
            if (!int.TryParse(gameID, out id))
            {
                status = Forbidden;
                return null;
            }

            // open connection to database
            using (SqlConnection connection = new SqlConnection(BoggleDB))
            {
                connection.Open();

                // execute all commands within a single transaction
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    string p1id = "";
                    string p2id = "";
                    bool completed = false;
                    FullStatusResponse response = new FullStatusResponse();

                    // figures out the GameState and records it
                    using (SqlCommand command = new SqlCommand("select * from Games where GameID = @GameID", connection, transaction))
                    {
                        command.Parameters.AddWithValue("@GameID", id);

                        // read the information the command returned
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows) // gameID wasn't in the database
                            {
                                status = Forbidden;
                                return null;
                            }

                            while (reader.Read())
                            {
                                // if the game only has one player, it's pending
                                if (DBNull.Value.Equals(reader["Player2"]))
                                {
                                    response.GameState = "pending";

                                    status = OK;
                                    return response;
                                }

                                response.Player1 = new SerialPlayer();
                                response.Player2 = new SerialPlayer();

                                //Grabbing info from the query
                                DateTime start = (reader["StartTime"] as DateTime?).GetValueOrDefault();
                                int limit = (reader["TimeLimit"] as int?).GetValueOrDefault();
                                string board = (string)reader["Board"];
                                completed = false;
                                int timeleft = (int)(start.AddSeconds(limit) - DateTime.UtcNow).TotalSeconds;

                                //Set the GameState and the timeleft
                                if (timeleft > 0)
                                {
                                    completed = false;
                                    response.GameState = "active";
                                    response.TimeLeft = timeleft;
                                }
                                else
                                {
                                    completed = true;
                                    response.GameState = "completed";
                                    response.TimeLeft = 0;
                                }

                                //Nonbrief requests have the board and timelimit
                                if (!briefbool)
                                {
                                    response.Board = board;
                                    response.TimeLimit = limit;
                                }

                                //Grabs the player ids
                                p1id = (string)reader["Player1"];
                                p2id = (string)reader["Player2"];

                            }
                        }
                    }

                    //Get p1 words
                    using (SqlCommand cmd = new SqlCommand("select * from Words where GameID = @GameID and Player = @Player", connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Player", p1id);
                        cmd.Parameters.AddWithValue("@GameID", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            IList<WordEntry> p1words = new List<WordEntry>();
                            int p1score = 0;

                            while (reader.Read())
                            {
                                int s = (int)reader["Score"];
                                p1words.Add(new WordEntry()
                                {
                                    Word = (string)reader["Word"],
                                    Score = s
                                });
                                p1score += s;
                            }

                            //Brief requests only get the score
                            if (briefbool)
                            {
                                response.Player1.Score = p1score;
                            }
                            //If the game is completed, get words and score
                            else if (completed == true)
                            {
                                response.Player1.Score = p1score;
                                response.Player1.WordsPlayed = p1words;
                            }
                            //If the game is active and brief is no, just give the score
                            else if (completed == false)
                            {
                                response.Player1.Score = p1score;
                            }

                        }
                    }

                    //get p2 words
                    using (SqlCommand cmd = new SqlCommand("select * from Words where GameID = @GameID and Player = @Player", connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@Player", p2id);
                        cmd.Parameters.AddWithValue("@GameID", id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            IList<WordEntry> p2words = new List<WordEntry>();
                            int p2score = 0;

                            while (reader.Read())
                            {
                                int s = (int)reader["Score"];
                                p2words.Add(new WordEntry()
                                {
                                    Word = (string)reader["Word"],
                                    Score = s
                                });
                                p2score += s;
                            }

                            //Brief requests only get the score
                            if (briefbool)
                            {
                                response.Player2.Score = p2score;
                            }
                            //If the game is completed, get words and score
                            else if (completed == true)
                            {
                                response.Player2.WordsPlayed = p2words;
                                response.Player2.Score = p2score;
                            }
                            //If the game is active and brief is no, just give the score
                            else if (completed == false)
                            {
                                response.Player2.Score = p2score;
                            }

                        }
                    }


                    //get p1 nickname if brief is no
                    if (!briefbool)
                    {
                        using (SqlCommand cmd = new SqlCommand("select * from Users where UserID = @id", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", p1id);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {

                                    response.Player1.Nickname = (string)reader["Nickname"];
                                }

                            }
                        }
                    }

                    //get p2 nickname if brief is no
                    if (!briefbool)
                    {
                        using (SqlCommand cmd = new SqlCommand("select * from Users where UserID = @id", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", p2id);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {

                                while (reader.Read())
                                {
                                    response.Player2.Nickname = (string)reader["Nickname"];
                                }

                            }
                        }
                    }

                    status = OK;
                    transaction.Commit();
                    return response;
                }
            }
        }
    }

    /// <summary>
    /// Contains helper static methods to perform various actions related to a Boggle game board.
    /// </summary>
    internal static class StaticBoard
    {
        /// <summary>
        /// Generates a new, random boggle board string
        /// </summary>
        public static string GenerateBoggleBoard()
        {
            string[] cubes =
            {
                "LRYTTE",
                "ANAEEG",
                "AFPKFS",
                "YLDEVR",
                "VTHRWE",
                "IDSYTT",
                "XLDERI",
                "ZNRNHL",
                "EGHWNE",
                "OATTOW",
                "HCPOAS",
                "OBBAOJ",
                "SEOTIS",
                "MTOICU",
                "ENSIEU",
                "NMIQHU"
            };

            // Shuffle the cubes
            Random r = new Random();
            for (int i = cubes.Length - 1; i >= 0; i--)
            {
                int j = r.Next(i + 1);
                string temp = cubes[i];
                cubes[i] = cubes[j];
                cubes[j] = temp;
            }

            // Make a string by choosing one character at random
            // frome each cube.
            string letters = "";
            for (int i = 0; i < cubes.Length; i++)
            {
                letters += cubes[i][r.Next(6)];
            }

            return letters;
        }

        /// <summary>
        /// Reports whether the provided word can be formed by tracking through
        /// this Boggle board as described in the rules of Boggle.  The method
        /// is case-insensitive.
        /// </summary>
        public static bool CanBeFormed(string boardString, string word)
        {
            // Construct a temporary char array representing the board
            char[,] board = new char[4, 4];
            int index = 0;

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    board[i, j] = boardString[index++];
                }
            }

            // Work in upper case
            word = word.ToUpper();

            // Mark every square on the board as unvisited.
            bool[,] visited = new bool[4, 4];

            // See if there is any starting point on the board from which
            // the word can be formed.
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (CanBeFormed(board, word, i, j, visited))
                    {
                        return true;
                    }
                }
            }

            // If no starting point worked, return false.
            return false;
        }

        /// <summary>
        /// Reports whether the provided word can be formed by tracking through
        /// this Boggle board by beginning at location [i,j] and avoiding any
        /// squares marked as visited.
        /// </summary>
        private static bool CanBeFormed(char[,] board, string word, int i, int j, bool[,] visited)
        {
            // If the word is empty, report success.
            if (word.Length == 0)
            {
                return true;
            }

            // If an index is out of bounds, report failure.
            if (i < 0 || i >= 4 || j < 0 || j >= 4)
            {
                return false;
            }

            // If this square has already been visited, report failure.
            if (visited[i, j])
            {
                return false;
            }

            // If the first letter of the word doesn't match the letter on
            // this square, report failure.  Otherwise, obtain the remainder
            // of the word that we should match next.
            // (Note that Q gets special treatment.)

            char firstChar = word[0];
            string rest = word.Substring(1);

            if (firstChar != board[i, j])
            {
                return false;
            }

            if (firstChar == 'Q')
            {
                if (rest.Length == 0)
                {
                    return false;
                }
                if (rest[0] != 'U')
                {
                    return false;
                }
                rest = rest.Substring(1);
            }

            // Mark this square as visited.
            visited[i, j] = true;

            // Try to match the remainder of the word, beginning at a neighboring square.
            if (CanBeFormed(board, rest, i - 1, j - 1, visited)) return true;
            if (CanBeFormed(board, rest, i - 1, j, visited)) return true;
            if (CanBeFormed(board, rest, i - 1, j + 1, visited)) return true;
            if (CanBeFormed(board, rest, i, j - 1, visited)) return true;
            if (CanBeFormed(board, rest, i, j + 1, visited)) return true;
            if (CanBeFormed(board, rest, i + 1, j - 1, visited)) return true;
            if (CanBeFormed(board, rest, i + 1, j, visited)) return true;
            if (CanBeFormed(board, rest, i + 1, j + 1, visited)) return true;

            // We failed.  Unmark this square and return false.
            visited[i, j] = false;
            return false;
        }
    }
}

