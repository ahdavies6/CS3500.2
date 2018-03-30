﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

/// <summary>
/// Collection of classes that represent the variety of request bodies that can be sent to the server
/// </summary>
namespace Boggle
{
    #region Request Structures
    /// <summary>
    /// Class the represents the request to make a user
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// Nickname passed to a create user request
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Creates a request with nickname
        /// </summary>
        /// <param name="nickname"></param>
        public CreateUserRequest(string nickname)
        {
            Nickname = nickname;
        }
    }

    /// <summary>
    /// Class that represents a join request
    /// </summary>
    public class JoinRequest
    {
        /// <summary>
        /// UserToken of the person who wants to join a game
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Time limit the user wants
        /// </summary>
        public string TimeLimit { get; set; }

        /// <summary>
        /// Creates a request with userToken and timeLimit
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="timeLimit"></param>
        public JoinRequest(string userToken, string timeLimit)
        {
            UserToken = userToken;
            TimeLimit = timeLimit;
        }
    }

    /// <summary>
    /// Represents the body of a cancel request
    /// </summary>
    public class CancelJoinRequest
    {
        /// <summary>
        /// UserToken of the person who wants to cancel a join request
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Creates a request with userToken
        /// </summary>
        /// <param name="userToken"></param>
        public CancelJoinRequest(string userToken)
        {
            UserToken = userToken;
        }
    }

    /// <summary>
    /// Class to hold the params for the request to play a word
    /// </summary>
    public class PlayWord
    {
        /// <summary>
        /// UserToken of the person putting in the play word request
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Word being played
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// Creates a request with userToken and word
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="word"></param>
        public PlayWord(string userToken, string word)
        {
            UserToken = userToken;
            Word = word;
        }
    }

    #endregion

    #region Response structures

    /// <summary>
    /// Sends back the UserToken
    /// </summary>
    public class UserTokenResponse
    {
        public string UserToken { get; set; }
    }


    /// <summary>
    /// Sends back the score produced by a play word request
    /// </summary>
    public class ScoreResponse
    {
        public int Score { get; set; }
    }

    /// <summary>
    /// Class that sends back the GameID of the game
    /// </summary>
    public class GameIDResponse
    {
        public string GameID { get; set; }
    }

    /// <summary>
    /// An interface to label what data structures can come from a Status response
    /// </summary>
    public interface Status
    {
    }

    /// <summary>
    /// Class for when status just needs to send "pending"
    /// </summary>
    public class StateResponse : Status
    {
        public string GameState { get; set; }
    }

    /// <summary>
    /// Class that represents the response from the server when get status is called
    /// </summary>
    [DataContract]
    public class FullStatusResponse : Status
    {
        [DataMember]
        public string GameState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Board { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }

        [DataMember]
        public int TimeLeft { get; set; }

        [DataMember]
        public SerialPlayer Player1 { get; set; }

        [DataMember]
        public SerialPlayer Player2 { get; set; }
    }

    /// <summary>
    /// Class that represents each player in the game when sending a status
    /// </summary>
    [DataContract]
    public class SerialPlayer
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }

        [DataMember]
        public int Score { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ISet<WordEntry> WordsPlayed { get; set; }
    }

    /// <summary>
    /// Represents a list of words that are played in the game when sent back as a status
    /// </summary>
    public class WordEntry
    {
        public string Word { get; set; }

        public int Score { get; set; }
    }
    #endregion
}