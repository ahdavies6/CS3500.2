﻿//Nithin Chalapathi u0847388 - University of Utah - Spring '18 - CS3500

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dependencies;

namespace DependencyGraphTestCases
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// An array of possible cell values as input to the dependency graph
        /// </summary>
        private string[] possibleVals = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j" };

        /////////////////////////////////////BEGIN BLACK BOX TESTING///////////////////////

        //////////Small value tests////////    

        /// <summary>
        /// Tests the size method for small values
        /// </summary>
        [TestMethod]
        public void TestSizeSmall()
        {
            DependencyGraph dg = new DependencyGraph();
            Assert.AreEqual(0, dg.Size);

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            Assert.AreEqual(possibleVals.Length - 1, dg.Size);

        }

        /// <summary>
        /// Tests the HasDependents(s) method. Tests the following cases:
        /// 1. Does not exist / graph is empty 
        /// 2. Only one dependee exists and has one dependent
        /// 3. Only one dependee exists and has multiple dependents
        /// 4. A fully populated graph
        /// 
        /// Also implicitly tests AddDependency(s,t)
        /// </summary>
        [TestMethod]
        public void TestHasDependentsSmall()
        {

            //Case 1
            DependencyGraph dg = new DependencyGraph();
            foreach (string s in possibleVals)
            {
                Assert.IsFalse(dg.HasDependents(s));
            }


            //Case 2
            dg = new DependencyGraph();
            dg.AddDependency(possibleVals[0], possibleVals[1]);
            Assert.IsTrue(dg.HasDependents(possibleVals[0]));

            //Case 3
            dg = new DependencyGraph();
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                dg.AddDependency(possibleVals[0], possibleVals[i]);
            }
            Assert.IsTrue(dg.HasDependents(possibleVals[0]));


            //Case 4
            dg = new DependencyGraph();

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            //All of the elements in possible vals but the last one has a dependant 
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                Assert.IsTrue(dg.HasDependents(possibleVals[i]));
            }

            //Last value does not have a dependent 
            Assert.IsFalse(dg.HasDependents(possibleVals[possibleVals.Length - 1]));

        }

        /// <summary>
        /// Tests the HasDependees(s) method. Tests the following cases:
        /// 1. Does not exist / graph is empty
        /// 2. Only one dependent exists and has one dependee
        /// 3. Only one dependent exists and has multiple depnedees 
        /// 4. A fully populated graph
        /// 
        /// Also implicitly tests AddDependency(s,t)
        /// </summary>
        [TestMethod]
        public void TestHasDependeesSmall()
        {
            //Case 1
            DependencyGraph dg = new DependencyGraph();
            foreach (string s in possibleVals)
            {
                Assert.IsTrue(dg.HasDependees(s));
            }

            //Case 2 
            dg = new DependencyGraph();
            dg.AddDependency(possibleVals[0], possibleVals[1]);
            Assert.IsTrue(dg.HasDependees(possibleVals[1]));

            //Case 3
            dg = new DependencyGraph();
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[0]);
            }
            Assert.IsTrue(dg.HasDependees(possibleVals[0]));

            //Case 4
            dg = new DependencyGraph();

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            //Checking all the values in possibleVals but the first element
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                Assert.IsTrue(dg.HasDependees(possibleVals[i]));
            }

            //First element that doesnt have any dependees
            Assert.IsTrue(dg.HasDependees(possibleVals[0]));

        }

        /// <summary>
        /// Tests the GetDependents(s) method. Tests the following cases:
        /// 1. Does not exist / graph is empty
        /// 2. Only one dependee exists and has one dependent
        /// 3. Only one dependee exists and has multiple dependents 
        /// 4. A fully populated graph
        /// 
        /// Also implicitly tests AddDependency(s,t)
        /// </summary>
        [TestMethod]
        public void TestGetDependentsSmall()
        {
            //Case 1
            DependencyGraph dg = new DependencyGraph();
            foreach (string s in possibleVals)
            {
                foreach (string t in dg.GetDependents(s))
                {
                    //If this code ever runs, it means that a dependent was found in an empty graph
                    Assert.Fail("Dependent found in an empty graph");
                }
            }

            //Case 2
            dg = new DependencyGraph();
            dg.AddDependency(possibleVals[0], possibleVals[1]);
            //Number of times the next loop runs
            int dependents = 0;

            foreach (string t in dg.GetDependents(possibleVals[0]))
            {
                Assert.Equals(possibleVals[1], t);
                dependents++;
            }

            //Means the loop ran for more / less than once
            if (dependents != 1)
            {
                Assert.Fail("GetDependents ran for: " + dependents + ". Not once.");
            }

            //Case 3
            dg = new DependencyGraph();
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                dg.AddDependency(possibleVals[0], possibleVals[i]);
            }

            dependents = 0;
            //Index to iterate through possibleVals to match with the dependents
            int index = 1;
            foreach (string t in dg.GetDependents(possibleVals[0]))
            {
                Assert.Equals(possibleVals[index], t);
                dependents++;
                index++;
            }

            if (dependents != possibleVals.Length - 1)
            {
                Assert.Fail("GetDependents ran for: " + dependents + ". Not" + (possibleVals.Length - 1));
            }

            //Case 4
            dg = new DependencyGraph();

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            dependents = 0;
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dependents = 0;
                foreach (string t in dg.GetDependents(possibleVals[i]))
                {
                    Assert.Equals(possibleVals[i], t);
                    dependents++;
                }

                //Each should only have 1 dependent
                Assert.Equals(1, dependents);
            }

            //Last element with no dependents
            dependents = 0;
            foreach (string t in dg.GetDependents(possibleVals[possibleVals.Length - 1]))
            {
                dependents++;
            }

            Assert.Equals(0, dependents);
        }

        /// <summary>
        /// Tests the GetDependees(s) method. Tests the following cases:
        /// 1. Does not exist / graph is empty
        /// 2. Only one dependent exists and has one dependee
        /// 3. Only one dependent exists and has multiple dependees 
        /// 4. A fully populated graph
        /// 
        /// Also implicitly tests AddDependency(s,t)
        /// </summary>
        [TestMethod]
        public void TestGetDependeesSmall()
        {
            //Case 1
            DependencyGraph dg = new DependencyGraph();
            foreach (string t in possibleVals)
            {
                foreach (string s in dg.GetDependees(t))
                {
                    //If this code ever runs, it means that a dependent was found in an empty graph
                    Assert.Fail("Dependee found in an empty graph");
                }
            }

            //Case 2
            dg = new DependencyGraph();
            dg.AddDependency(possibleVals[0], possibleVals[1]);
            //Number of times the next loop runs
            int dependees = 0;

            foreach (string s in dg.GetDependees(possibleVals[1]))
            {
                Assert.Equals(possibleVals[0], s);
                dependees++;
            }

            //Means the loop ran for more / less than once
            if (dependees != 1)
            {
                Assert.Fail("GetDependees ran for: " + dependees + ". Not once.");
            }

            //Case 3
            dg = new DependencyGraph();
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[0]);
            }

            dependees = 0;
            //Index to iterate through possibleVals to match with the dependees
            int index = 1;
            foreach (string s in dg.GetDependees(possibleVals[0]))
            {
                Assert.Equals(possibleVals[index], s);
                dependees++;
                index++;
            }

            if (dependees != possibleVals.Length - 1)
            {
                Assert.Fail("GetDependees ran for: " + dependees + ". Not" + (possibleVals.Length - 1));
            }

            //Case 4
            dg = new DependencyGraph();

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            dependees = 0;
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                dependees = 0;
                foreach (string s in dg.GetDependees(possibleVals[i]))
                {
                    Assert.Equals(possibleVals[i], s);
                    dependees++;
                }

                //Each should only have 1 dependee
                Assert.Equals(1, dependees);
            }

            //First element with not depenedees 
            dependees = 0;
            foreach (string t in dg.GetDependents(possibleVals[0]))
            {
                dependees++;
            }

            Assert.Equals(0, dependees);
        }

        /// <summary>
        /// Tests removing and adding depedencies using the following cases:
        /// 
        /// 1. empty graph, add one depedency, then remove that dependency
        /// 2. Graph with 1 dependency, add another, then remove the original
        /// 3. Start with an empty graph, build it fully, then completly remove all dependencies
        /// 
        /// Implicitly tests GetDependents, GetDependees, Size
        /// </summary>
        [TestMethod]
        public void TestAddAndRemoveDependencySmall()
        {
            //Case 1
            DependencyGraph dg = new DependencyGraph();
            dg.AddDependency(possibleVals[0], possibleVals[1]);
            dg.RemoveDependency(possibleVals[0], possibleVals[1]);
            Assert.Equals(0, dg.Size);

            //Case 2
            dg = new DependencyGraph();
            dg.AddDependency(possibleVals[0], possibleVals[1]);
            Assert.Equals(1, dg.Size);
            dg.AddDependency(possibleVals[1], possibleVals[2]);
            Assert.Equals(2, dg.Size);
            dg.RemoveDependency(possibleVals[0], possibleVals[1]);
            Assert.Equals(1, dg.Size);

            foreach (string t in dg.GetDependents(possibleVals[1]))
            {
                Assert.Equals(possibleVals[2], t);
            }

            foreach (string s in dg.GetDependees(possibleVals[2]))
            {
                Assert.Equals(possibleVals[1], s);
            }

            //Case 3
            dg = new DependencyGraph();

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            Assert.Equals(possibleVals.Length - 1, dg.Size);

            //Making sure the dependees are there
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                foreach (string s in dg.GetDependees(possibleVals[i]))
                {
                    Assert.Equals(possibleVals[i - 1], s);
                }
            }

            //Making sure the dependents are there
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                foreach (string t in dg.GetDependents(possibleVals[i]))
                {
                    Assert.Equals(possibleVals[i + 1], t);
                }
            }

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.RemoveDependency(possibleVals[i], possibleVals[i + 1]);
            }

            Assert.Equals(0, dg.Size);

            //Making sure the dependees are there
            for (int i = 1; i < possibleVals.Length - 1; i++)
            {
                foreach (string s in dg.GetDependees(possibleVals[i]))
                {
                    Assert.Fail("There should be no dependees after the graph is cleared.");
                }
            }

            //Making sure the dependents are there
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                foreach (string t in dg.GetDependents(possibleVals[i]))
                {
                    Assert.Fail("There should be no dependents after the graph is cleared.");
                }
            }

        }

        /// <summary>
        /// Tests ReplaceDependents
        /// 
        /// Creates a full graph and replaces all of the dependents with one new dependent using 2 cases:
        /// 
        /// 1. A graph that is a full, attempts to make the dependent the same for all
        /// 2. In a full graph, attempts to add an entirely new set of dependents
        /// 
        /// Implicitly tests GetDependees, GetDependents,s AddDependency 
        /// </summary>
        [TestMethod]
        public void TestReplaceDependentsSmall()
        {
            //Case 1 
            DependencyGraph dg = new DependencyGraph();
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            //Making a IEnumerable
            string[] newDependents = { possibleVals[possibleVals.Length - 1] };

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.ReplaceDependents(possibleVals[i], newDependents);
            }

            //Creating a copy of the array that doesnt include the single dependent but has all the dependees
            string[] checkDependees = (string[])possibleVals.Clone();
            checkDependees[checkDependees.Length - 1] = null;
            int numDependees = 0;

            foreach (string s in dg.GetDependees(possibleVals[possibleVals.Length - 1]))
            {
                numDependees++;
                for (int i = 0; i < checkDependees.Length - 2; i++)
                {
                    if (!(checkDependees[i] is null) && s.Equals(checkDependees[i]))
                    {
                        checkDependees[i] = null;
                    }
                }
            }

            //Check all of the checkDependees is null and the foreach only ran the proper amount of times
            foreach (string elem in checkDependees)
            {
                Assert.Equals(null, elem);
            }
            Assert.Equals(checkDependees.Length - 1, numDependees);



            //Case 2
            dg = new DependencyGraph();
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            string[] AddDeps = { "1", "2", "3", "4", "5", "6" };

            dg.ReplaceDependents(possibleVals[0], AddDeps);

            //to keep track of where in addDeps we are
            int index = 0;
            foreach (string t in dg.GetDependents(possibleVals[0]))
            {
                Assert.Equals(AddDeps[index], t);
                index++;
            }

        }

        /// <summary>
        /// Tests ReplaceDependees(s)
        /// 
        /// Creates a full graph and replaces all of the dependees with one new dependent using 2 cases:
        /// 
        /// 1. A graph that is a full, attempts to make the dependee the same for all
        /// 2. In a full graph, attempts to add an entirely new set of dependees
        /// 
        /// Implicitly tests GetDependees, GetDependents,s AddDependency 
        /// </summary>
        [TestMethod]
        public void TestReplaceDependeesSmall()
        {
            //Case 1 
            DependencyGraph dg = new DependencyGraph();
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            //Making a IEnumerable
            string[] newDependees = { possibleVals[possibleVals.Length - 1] };

            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.ReplaceDependees(possibleVals[i], newDependees);
            }

            //Creating a copy of the array that doesnt include the single dependent but has all the dependees
            string[] checkDependents = (string[])possibleVals.Clone();
            checkDependents[checkDependents.Length - 1] = null;
            int numDependents = 0;

            foreach (string s in dg.GetDependents(possibleVals[possibleVals.Length - 1]))
            {
                numDependents++;
                for (int i = 0; i < checkDependents.Length - 2; i++)
                {
                    if (!(checkDependents[i] is null) && s.Equals(checkDependents[i]))
                    {
                        checkDependents[i] = null;
                    }
                }
            }

            //Check all of the checkDependents is null and the foreach only ran the proper amount of times
            foreach (string elem in checkDependents)
            {
                Assert.Equals(null, elem);
            }
            Assert.Equals(checkDependents.Length - 1, numDependents);



            //Case 2
            dg = new DependencyGraph();
            for (int i = 0; i < possibleVals.Length - 2; i++)
            {
                dg.AddDependency(possibleVals[i], possibleVals[i + 1]);
            }

            string[] AddDeps = { "1", "2", "3", "4", "5", "6" };

            dg.ReplaceDependees(possibleVals[0], AddDeps);

            //to keep track of where in addDeps we are
            int index = 0;
            foreach (string t in dg.GetDependees(possibleVals[0]))
            {
                Assert.Equals(AddDeps[index], t);
                index++;
            }

        }
    }
}
