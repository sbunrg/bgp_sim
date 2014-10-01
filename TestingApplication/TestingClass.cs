﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using SecureSimulator;
using CloudLibrary;


namespace TestingApplication
{
    /// <summary>
    /// a bunch of functions to help us out with testing that the program is running as expected.
    /// </summary>
    class TestingClass
    {

        /// <summary>
        /// this is hardcoded to be like fig. 20 in the Feb version of the sigcomm 2010 paper.
        /// </summary>
        /// <returns></returns>
        public NetworkGraph getTestGraph()
        {
            NetworkGraph testGraph = new NetworkGraph();

            testGraph.AddEdge(1, 2, RelationshipType.PeerOf);
            testGraph.AddEdge(2, 1, RelationshipType.PeerOf);
            testGraph.AddEdge(1, 3, RelationshipType.CustomerOf);
            testGraph.AddEdge(3, 1, RelationshipType.ProviderTo);
            testGraph.AddEdge(1, 9, RelationshipType.ProviderTo);
            testGraph.AddEdge(9, 1, RelationshipType.CustomerOf);
            testGraph.AddEdge(1, 4, RelationshipType.PeerOf);
            testGraph.AddEdge(4, 1, RelationshipType.PeerOf);
            testGraph.AddEdge(3, 4, RelationshipType.ProviderTo);
            testGraph.AddEdge(4, 3, RelationshipType.CustomerOf);
            testGraph.AddEdge(3, 7, RelationshipType.ProviderTo);
            testGraph.AddEdge(7, 3, RelationshipType.CustomerOf);
            testGraph.AddEdge(7, 4, RelationshipType.PeerOf);
            testGraph.AddEdge(4, 7, RelationshipType.PeerOf);
            testGraph.AddEdge(4, 8, RelationshipType.ProviderTo);
            testGraph.AddEdge(8, 4, RelationshipType.CustomerOf);
            testGraph.AddEdge(2, 9, RelationshipType.ProviderTo);
            testGraph.AddEdge(9, 2, RelationshipType.CustomerOf);
            testGraph.AddEdge(2, 5, RelationshipType.PeerOf);
            testGraph.AddEdge(5, 2, RelationshipType.PeerOf);
            testGraph.AddEdge(2, 6, RelationshipType.ProviderTo);
            testGraph.AddEdge(6, 2, RelationshipType.CustomerOf);
            testGraph.AddEdge(5, 6, RelationshipType.ProviderTo);
            testGraph.AddEdge(6, 5, RelationshipType.CustomerOf);

            return testGraph;
        }

        public string getTestingGraphFilename()
        {
            return "C:\\Users\\pgill\\Desktop\\adoptTraffic\\Data\\Cyclops\\Cyclops_2009_IXP.txt";
        }

        public void CLI(bool script)
        {
            string resp;
            List<Destination> d = new List<Destination>();
            NetworkGraph g = new NetworkGraph();
            Worker w = new Worker();
            bool[] S = new bool[Constants._numASNs];
            for (int i = 0; i < S.Length; i++)
                S[i] = false;
            GlobalState globalState = new GlobalState();
            globalState.S = S;
            StreamReader input = new StreamReader( Console.OpenStandardInput());
            if (script)
            {
                Console.WriteLine("You have selected to run a script, please enter the file name:");
                string scriptfile = Console.ReadLine();
                if (!File.Exists(scriptfile))
                    Console.WriteLine("script file: " + scriptfile + " did not exist, entering interactive mode");
                else
                    input = new StreamReader(scriptfile);
            }
            globalState.W = new UInt16[Constants._numASNs];
            for (int i = 0; i < globalState.W.Length; i++)
                globalState.W[i] = 1;
            

            Console.WriteLine("Welcome to the testing interface: (type help for help) 66000 ");
            bool exitNow = false;
            
            
            while(!exitNow)
            {
                if (input.EndOfStream)
                {
                    input.Close();
                    input = new StreamReader(Console.OpenStandardInput());
                    Console.WriteLine("script has ended, now in interactive mode");
                }
                Console.Write(">>");
				string command = input.ReadLine ().ToLower();
                if (command.IndexOf("input") == 0)
                {
                    initGraph(ref g, command);
                    globalState.nonStubs = g.getNonStubs();
                }
                else if (command.IndexOf("destination") == 0)
                {

                    if (command.IndexOf("all") < 0)
                    {
						Stopwatch stopwatch = new Stopwatch();
						stopwatch.Reset();
						stopwatch.Start();
                        Destination newD = new Destination();
                        if (initDestination(ref g, ref newD, command))
                        {
                            d.Add(newD);
							stopwatch.Stop();
							Console.WriteLine("initialized and added " + newD.destination);
							Console.WriteLine("Tree generation took (ms): " + stopwatch.ElapsedMilliseconds);

                        }
                    }
                    else
                    {
                        IEnumerable<AsNode> allASes = g.GetAllNodes();
                        foreach (AsNode AS in allASes)
                        {
                            Destination newD = new Destination();
                            if (initDestination(ref g, ref newD, "destination " + AS.NodeNum))
                            {
                                d.Add(newD);
                                Console.WriteLine("initialized and added " + newD.destination);
                            }
                        }
                    }

                }
                else if (command.IndexOf("resultsexplorer") == 0)
                {
                    ResultsExplorer res = new ResultsExplorer();
                    res.ResultsInterface();
                }
                else if (command.IndexOf("setstate") == 0)
                    initS(ref  globalState.S, command);
                else if (command.IndexOf("addedges") == 0)
                    addEdges(command, ref g);
                else if (command.IndexOf("getlink") == 0)
                    getLink(command, ref g);
                else if (command.IndexOf("flipallu") == 0)
                    flipallU(ref d, ref g, ref globalState, command);
                else if (command.IndexOf("printstate") == 0)
                    printState(ref  globalState.S, command);
                else if (command.IndexOf("printsecp") == 0)
                    printSecP(ref d, command);
               
                else if (command.IndexOf("getl") == 0)
                    getL(ref d, command);
                else if (command.IndexOf("getw") == 0)
                    getW(ref globalState.W, command);
                else if (command.IndexOf("printw") == 0)
                    printWeight(ref globalState.W, command);
                else if (command.IndexOf("setw") == 0)
                    setW(ref globalState.W, command);
                else if (command.IndexOf("getbest") == 0)
                    getBest(ref d, command);
                else if (command.IndexOf("getpath") == 0)
                    getPath(ref d, command);
                else if (command.IndexOf("getsecp") == 0)
                    getSecP(ref d, command);
                else if (command.IndexOf("getutility") == 0)
                    getUtility(ref d, command);
                else if (command.IndexOf("onlynonstubs") == 0)
                    SimulatorLibrary.setOnlyStubs(true);
                else if (command.IndexOf("notonlynonstubs") == 0)
                    SimulatorLibrary.setOnlyStubs(false);
                else if (command.IndexOf("iterateall") == 0)
                    iterateAll(ref d, ref g, ref globalState, command);
                else if (command.IndexOf("iterate") == 0)
                    iterate(ref d, ref  globalState, command);
                else if (command.IndexOf("not") == 0)
                    computeNotN(ref d, ref globalState, ref w, command);
                else if (command.IndexOf("wgetsecp") == 0)
                    getWorkerSecP(ref w, command);
                else if (command.IndexOf("initglobalstate") == 0)
                    initGlobalState(command, ref g, ref globalState);
                else if (command.IndexOf("wgetpath") == 0)
                    getWorkerPath(ref w, command);
                else if (command.IndexOf("wgetparent") == 0)
                    getWorkerParent(ref w, command);
                else if (command.IndexOf("printbuckettable") == 0)
                    printBucketTable(ref d, command);
                else if (command.IndexOf("compareu") == 0)
                    compareU(ref d, ref  globalState, ref g, command);
                else if (command.IndexOf("flipu") == 0)
                    flipU(ref d, ref g, ref globalState, command);
                else if (command.IndexOf("getnonstubs") == 0)
                    printnonstubs(ref g);
                else if (command.IndexOf("getstubs") == 0)
                    printstubs(ref g);
                else if (command.IndexOf("turnonstubs") == 0)
                    turnOnStubs(ref g, ref  globalState.S);
                else if (command.IndexOf("getcustomers") == 0)
                    getcustomers(ref g, command);
                else if (command.IndexOf("getpeers") == 0)
                    getpeers(ref g, command);
                else if (command.IndexOf("getproviders") == 0)
                    getproviders(ref g, command);
                else if (command.IndexOf("gets") == 0) //must be tested for after getsecp
                    getS(ref  globalState.S, command);
                else if (command.IndexOf("sets") == 0) //this must be tested for *after* the test for setstate
                    setS(ref  globalState.S, command);
                else if (command.IndexOf("computehash") == 0)
                    computeHash(command);
                else if (command.IndexOf("setutilitycomputation") == 0)
                    setUtilityComputation(command);
                else if (command.IndexOf("numberon") == 0)
                    numberOn(ref  globalState.S);
                else if (command.IndexOf("getaveragebest") == 0)
                    getAverageBest(command, ref d);
                else if (command.IndexOf("nodeswithnopath") == 0)
                    nodesWithNoPath(command, ref d, ref g);
                else if (command.IndexOf("traversedod") == 0)
                    DoDAnaly.traverseDoD(g);
                else if (command.IndexOf("clear") == 0)
                {
                    Console.WriteLine("clearing state of graph, destination, S and worker.");

                    g = new NetworkGraph();
                    for (int i = 0; i < S.Length; i++)
                        S[i] = false;
                    d = new List<Destination>();
                    w = new Worker();
                }
                else if (command.IndexOf("help") == 0)
                    help();
                else if (command.IndexOf("list") == 0)
                {
                    Console.WriteLine("printing current directory contents:");
                    string[] dirContents = Directory.GetFiles(".");
                    foreach (string s in dirContents)
                        Console.WriteLine(s);
                }
                else if (command.IndexOf("exit") == 0)
                    exitNow = true;
             
                Console.Write(">>");
            }
            
            input.Close();

        }

        private bool addEdges(string command, ref NetworkGraph g)
        {
            string[] pieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            if (pieces.Length < 4)
            {
                Console.WriteLine("usage: addEdges ASN numEdges onlyNonStubs");
                return false;
            }
            UInt32 ASN;
            Int32 numEdges;
            bool onlyNonStubs;
            if (!UInt32.TryParse(pieces[1], out ASN) || !int.TryParse(pieces[2], out numEdges) || !bool.TryParse(pieces[3], out onlyNonStubs))
            {
                Console.WriteLine("bad inputs!");
                return false;
            }

            SimulatorLibrary.addEdges(ref g, ASN, numEdges, onlyNonStubs);
            return true;

        }
        private bool getLink(string command, ref NetworkGraph g)
        {
            string[] pieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 3)
            {
                Console.WriteLine("usage: getLink <AS1> <AS2>");
                return false;
            }
            uint as1, as2;
            if (uint.TryParse(pieces[1], out as1) && uint.TryParse(pieces[2], out as2))
            {
                AsNode asnode1 = g.GetNode(as1);
                AsNode asnode2 = g.GetNode(as2);
                if (asnode1.GetAllNeighbors().Contains(asnode2))
                {
                    RelationshipType rt = asnode1.GetRelationshipTypeOfNeighbor(asnode2);
                    Console.WriteLine("AS " + as1 + " has relationship " + rt + " with AS " + as2);
                    return true;
                }
                else
                {
                    
                    Console.WriteLine("edge was not in graph");
                }
                return true;
            }
            return true;
        }
        private bool initGlobalState(string command, ref NetworkGraph g, ref GlobalState globalState)
        {
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("usage: initglobalstate :early adopters:top x:top x%");
                Console.WriteLine("e.g., initglobalstate :15169 8075:8075 22822:40 will set security true for 15169 and 8075 and assign 40 % weight total to 8075 and 22822 with remaining 60% divided across other ASs");
                return false;
            }

            string [] parameters= command.Split(":".ToCharArray(),StringSplitOptions.RemoveEmptyEntries);

            if (parameters.Length != 4)
            {
                Console.WriteLine("usage: initglobalstate :early adopters:top x:top x%");
                Console.WriteLine("e.g., initglobalstate :15169 8075:8075 22822:40 will set security true for 15169 and 8075 and assign 40 % weight total to 8075 and 22822 with remaining 60% divided across other ASs");
                return false;
            }

            List<UInt32> earlyAdopters = new List<UInt32>();
            string[] earlyAdopterStrings = parameters[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string eAS in earlyAdopterStrings)
            {
                UInt32 currAdopter;
                if (UInt32.TryParse(eAS, out currAdopter))
                {
                    if (g.GetNode(currAdopter) != null)
                        earlyAdopters.Add(currAdopter);
                    else
                        Console.WriteLine(currAdopter + " was not in the graph.");
                }
                else
                    Console.WriteLine("could not parse AS: " + eAS);
            }
            List<UInt32> topX = new List<UInt32>();
            string[] topXStrings = parameters[2].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string tXS in topXStrings)
            {
                UInt32 currTopX;
                if (UInt32.TryParse(tXS, out currTopX))
                {
                    if (g.GetNode(currTopX) != null)
                        topX.Add(currTopX);
                    else
                        Console.WriteLine(currTopX + " was not in the graph.");
                }
                else
                    Console.WriteLine("could not parse AS: " + tXS);
            }

            short topXPercent;
            if (!short.TryParse(parameters[3], out topXPercent))
            {
                Console.WriteLine("could not parse your top X percent value!");
                return false;
            }

            Console.WriteLine("dividing " + topXPercent + " across ");
            foreach (UInt32 topXAS in topX)
                Console.Write(topXAS + ", ");
            Console.WriteLine();
            Console.WriteLine("turning on stubs and ");
            foreach (UInt32 adopter in earlyAdopters)
                Console.Write(adopter + ", ");
            Console.WriteLine();

            globalState = SimulatorLibrary.initGlobalState(g, earlyAdopters, topX, topXPercent);
            return true;
        }

        private bool nodesWithNoPath(string command, ref List<Destination> destinations,ref NetworkGraph g)
        {
            string[] parameters = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (parameters.Length < 2)
            {
                Console.WriteLine("error: usage averagebest [destination]");
                return false;
            }
            UInt32 ASN;
            if (!UInt32.TryParse(parameters[1], out ASN))
            {
                Console.WriteLine("error: invalid destination ASN");
                return false;
            }
            foreach (Destination d in destinations)
            {
                if (d.destination == ASN)
                {
                    Int32 numWithNoPath=0;
                    IEnumerable<AsNode> allNodes = g.GetAllNodes();
                    foreach (AsNode AS in allNodes)
                    {
                        if (d.Best[AS.NodeNum] == null)
                            numWithNoPath++;
                        
                    }

                    Console.WriteLine(numWithNoPath + " nodes have no path to destination " + d.destination);
                }
            }

            Console.WriteLine("error could not find AS " + ASN + " in the list of destinations");
            return false;
        }

        private bool getAverageBest(string command, ref List<Destination> destinations)
        {
            string[] parameters = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (parameters.Length < 2)
            {
                Console.WriteLine("error: usage averagebest [destination]");
                return false;
            }
            UInt32 ASN;
            if (!UInt32.TryParse(parameters[1], out ASN))
            {
                Console.WriteLine("error: invalid destination ASN");
                return false;
            }
            foreach (Destination d in destinations)
            {
                if (d.destination == ASN)
                {
                    Console.WriteLine("Average Best for destination " + ASN + " is " + d.averageBestSize());
                    return true;
                }
            }

            Console.WriteLine("error could not find AS " + ASN + " in the list of destinations");
            return false;
        }
       
        private bool setUtilityComputation(string command)
        {
            string[] parameters = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (parameters.Length < 2)
            {
                Console.WriteLine("error usage: setutilitycompation [basic|incoming|outgoing|source]");
                return false;
            }

            switch (parameters[1])
            {
                case "basic":
                    SimulatorLibrary.setUtilityComputation(UtilityComputationType.basic);
                    Console.WriteLine("utility computation is now basic");
                    break;
                case "incoming":
                    SimulatorLibrary.setUtilityComputation(UtilityComputationType.incoming);
                    Console.WriteLine("utility computation is now incoming");
                    break;
                case "outgoing":
                    SimulatorLibrary.setUtilityComputation(UtilityComputationType.outgoing);
                    Console.WriteLine("utility computation is now outgoing");
                    break;
                case "source":
                    SimulatorLibrary.setUtilityComputation(UtilityComputationType.source);
                    Console.WriteLine("utility compuation is now source.");
                    break;
                default:
                    Console.WriteLine("Error: could not ID the utility computation type.");
                    return false;
                    
            }
            return true;
        }

        private bool iterateAll(ref List<Destination> d, ref NetworkGraph g, ref GlobalState globalState, string command)
        {
            string[] parameters = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            float percentage = 0;
            if (parameters.Length == 1)
                Console.WriteLine("no utility percentage specified, using basic condition that U after must be more than U before");
            else if (!float.TryParse(parameters[1], out percentage))
            {
                percentage = 0;
                Console.WriteLine("could not parse the utility percentage. using default condition Uafter > Ubefore to flip");
            }
            bool useMiniDWeight = false;
           
            if (parameters.Length > 2 && parameters[2].IndexOf("t")>=0)
            {
                useMiniDWeight = true;
                Console.WriteLine("using mini d weight.");
            }
           
            List<Message> results = new List<Message>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            foreach (Destination dest in d)
            {
                //yes this is pretty dumb/inefficient it is just for testing purposes.
                MiniDestination mD = SimulatorLibrary.initMiniDestination(g, dest.destination, false);
                
                results.Add(SimulatorLibrary.ComputeOnDestination(mD, globalState));
            }
            Int64[] Before = new Int64[Constants._numASNs];
            Int64[] After = new Int64[Constants._numASNs];
           SimulatorLibrary.updateGlobalState(ref globalState, results, percentage,ref Before,ref After);
           stopwatch.Stop();
           Console.WriteLine("iteration took: " + stopwatch.ElapsedMilliseconds);

            return true;

        }

        private bool getpeers(ref NetworkGraph g, string command)
        {
            string resp;
            bool onlystubs = false;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the peers of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                onlystubs = true;
                Console.WriteLine("you have indicated you are only interested in stub networks.");
            }
            UInt32 ASN;
            if (!UInt32.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }
            AsNode n;
            List<UInt32> stubs = g.getStubs();
            if ((n = g.GetNode(ASN)) != null)
            {
                Console.WriteLine("Peers of AS: " + ASN + " are:");
                foreach (AsNode customer in n.GetNeighborsByType(RelationshipType.PeerOf))
                {
                    if (!onlystubs || (stubs.Contains(customer.NodeNum)))
                    Console.Write(customer.NodeNum + " : ");
                }
                Console.WriteLine("AS has: " + n.GetNeighborsByType(RelationshipType.PeerOf).Count() + " peers, " + n.GetNeighborsByType(RelationshipType.CustomerOf).Count() + " providers, and " + n.GetNeighborsByType(RelationshipType.ProviderTo).Count() + " customers");
                Console.WriteLine();

            }
            else
            {
                Console.WriteLine("AS: " + ASN + " was not found in the graph, are you sure it exists?");
                return false;
            }

            return true;
        }

        private bool getproviders(ref NetworkGraph g, string command)
        {
            string resp;
            bool onlystubs = false;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the providers of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                onlystubs = true;
                Console.WriteLine("you have indicated you are only interested in stub networks.");
            }
            UInt32 ASN;
            if (!UInt32.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }
            AsNode n;
            List<UInt32> stubs = g.getStubs();
            if ((n = g.GetNode(ASN)) != null)
            {
                Console.WriteLine("Providers of AS: " + ASN + " are: (ie. AS is a customer of)");
                foreach (AsNode customer in n.GetNeighborsByType(RelationshipType.CustomerOf))
                {
                    if (!onlystubs || (stubs.Contains(customer.NodeNum)))
                    Console.Write(customer.NodeNum + " : ");
                }
                Console.WriteLine();

            }
            else
            {
                Console.WriteLine("AS: " + ASN + " was not found in the graph, are you sure it exists?");
                return false;
            }

            return true;
        }

        private bool getcustomers(ref NetworkGraph g, string command)
        {
            string resp;
            bool onlystubs=false;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the customers of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                onlystubs = true;
                Console.WriteLine("you have indicated you are only interested in stub networks.");
            }

            //parse out the ASN of interest.
            UInt32 ASN;
            if (!UInt32.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }


            List<UInt32> stubs = g.getStubs();

            AsNode n;
            if ((n = g.GetNode(ASN)) != null)
            {
                Console.WriteLine("Customers of AS: " + ASN + " are: (ie. AS provides to)");
                foreach (AsNode customer in n.GetNeighborsByType(RelationshipType.ProviderTo))
                {
                    if(!onlystubs || (stubs.Contains(customer.NodeNum)))
                    Console.Write(customer.NodeNum + " : ");
                }
                Console.WriteLine();

            }
            else
            {
                Console.WriteLine("AS: " + ASN + " was not found in the graph, are you sure it exists?");
                return false;
            }

            return true;
        }

        private bool printnonstubs(ref NetworkGraph g)
        {
            Console.WriteLine("printing non stubs:");
            List<UInt32> nonstubs = g.getNonStubs();
            for (int i = 0; i < nonstubs.Count; i++)
                Console.WriteLine(nonstubs[i]);

            return true;
        }

        private bool printstubs(ref NetworkGraph g)
        {
            Console.WriteLine("printing stubs:");
            List<UInt32> stubs = g.getStubs();
            for (int i = 0; i < stubs.Count; i++)
                Console.WriteLine(stubs[i]);

            return true;
        }

        private bool turnOnStubs(ref NetworkGraph g, ref bool[] S)
        {
            //clear S
            for (int i = 0; i < S.Length; i++)
                S[i] = false;
            List<UInt32> stubs = g.getStubs();
            foreach (int stub in stubs)
            {
                S[stub] = true;
            }

            return true;
        }

        private bool compareU(ref List<Destination> ds, ref GlobalState globalState,ref NetworkGraph g,string command)
        {
            string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 2)
            {
                Console.WriteLine("usage: compareu <d> <onlybenefit?>");
                return false;
            }
            /** first off try to parse out a specific destination (or all) **/
            
            UInt32 destNum;
            if (!UInt32.TryParse(cpieces[1], out destNum) && !cpieces[1].Equals("all"))
            {
                Console.WriteLine("invalid destination number");
                return false;
            }
            List<Destination> destinationsToCompare = new List<Destination>();
            if (cpieces[1].Equals("all"))
            {
                //want to run on all destinations.
                destinationsToCompare = ds;
            }
            else
            {
                foreach (Destination curr in ds)
                {
                    if (curr.destination == destNum)
                        destinationsToCompare.Add(curr);
                }
            }
            /** next see if we have been asked only for nodes that benefit **/
            bool onlybenefit = false;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                Console.WriteLine("you've chosen to only display ASes that benefit");
                onlybenefit = true;
            }
          
 
            List<UInt32> nonstubs = g.getNonStubs();
            Worker w = new Worker();
            foreach (Destination d in destinationsToCompare)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Console.WriteLine("Destination : " + d.destination);
                Console.WriteLine("ASN :: U(before) : U(flipped)");
                runCompareU(nonstubs, d, ref w, onlybenefit,ref globalState);
                stopwatch.Stop();
                Console.WriteLine("compare u took " + stopwatch.ElapsedMilliseconds + " ms");
            }
            return true;
        }

        private void runCompareU(List<UInt32> nonstubs,Destination d,ref Worker w,bool onlybenefit,ref GlobalState globalState)
        {
            if (d.BucketTable == null)
            {
                Console.WriteLine("null bucket table!");
                return;
            }
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Reset();
            stopwatch.Start();
            d.UpdatePaths(globalState.S);
            d.ComputeU(globalState.W);//just in case.
            stopwatch.Stop();
           
            foreach (UInt32 ASN in nonstubs)
            {
                if (d.Best[ASN] != null)//nonstub has a path to d
                {  
                    int notASN = w.ComputeUtility(d.BucketTable, d.Best, d.ChosenParent, d.SecP, globalState.S, ASN, d.L[ASN], d.BestRelation[ASN], globalState.W);
                    if (!onlybenefit || d.U[ASN] < notASN)
                        Console.WriteLine(ASN + " :: " + d.U[ASN] + " : " + notASN);
                }
            }
            Console.WriteLine("update paths and compute u took: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        private bool numberOn(ref bool[] S)
        {
            int numOn = 0;
            foreach (bool s in S)
            {
                if (s)
                    numOn++;
            }

            Console.WriteLine(numOn + " ASes have security turned on");
            return true;
        }

        private bool flipU(ref List<Destination> ds, ref NetworkGraph g, ref GlobalState globalState,string command)
        {
            Console.WriteLine("Flipping state:");

            string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 2)
            {
                Console.WriteLine("usage: flipu <d> <quiet?>");
                return false;
            }

            Destination d = new Destination();
            UInt32 destNum;
            if (!UInt32.TryParse(cpieces[1], out destNum))
            {
                Console.WriteLine("invalid destination number");
                return false;
            }
            foreach (Destination curr in ds)
            {
                if (curr.destination == destNum)
                    d = curr;
            }
            if (d.BucketTable == null)
            {
                Console.WriteLine("null bucket table!");
                return false;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<UInt32> toflip = new List<UInt32>();
            bool quiet = false;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 2)
            {
                quiet = true;
                Console.WriteLine("you have selected quiet mode");
            }

            List<UInt32> nonstubs = g.getNonStubs();
            foreach (UInt32 ASN in nonstubs)
            {
                if (d.Best[ASN] != null)//make sure this AS has a route to our destination
                {
                    Worker w = new Worker();
                    int notASN = w.ComputeUtility(d.BucketTable, d.Best, d.ChosenParent, d.SecP, globalState.S, ASN, d.L[ASN], d.BestRelation[ASN], globalState.W);
                    if (d.U[ASN] < notASN)
                    {
                        if (!quiet)
                            Console.WriteLine("AS: " + ASN + " from " + globalState.S[ASN] + " to " + !globalState.S[ASN]);
                        toflip.Add(ASN);//don't flip on the fly it messes up everyones calculations, do it at the end.
                    }
                }
            }


            foreach (int ASN in toflip)
            {
                if (globalState.S[ASN])
                    Console.WriteLine("AS: " + ASN + " is rolling back!!!");
                globalState.S[ASN] = !globalState.S[ASN];
            
            }
            stopwatch.Stop();
            Console.WriteLine("flip u took " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("it flipped " + toflip.Count + " ASes");
            return true;
        }

        private bool flipallU(ref List<Destination> ds, ref NetworkGraph g, ref GlobalState globalState, string command)
        {
            Console.WriteLine("Flipping state:");

            string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 1)
            {
                Console.WriteLine("usage: flipallu <quiet?>");
                return false;
            }

           
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<UInt32> toflip = new List<UInt32>();
            bool quiet = false;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 1)
            {
                quiet = true;
                Console.WriteLine("you have selected quiet mode");
            }
            List<UInt32> nonstubs = g.getNonStubs();
            Int64[] deltaU = new Int64[Constants._numASNs];
            for (int i = 0; i < deltaU.Length; i++)
                deltaU[i] = 0;

            foreach (Destination d in ds)
            {
                foreach (UInt32 ASN in nonstubs)
                {
                    if (d.Best[ASN] != null)//make sure this AS has a route to our destination
                    {
                        Worker w = new Worker();
                        int notASN = w.ComputeUtility(d.BucketTable, d.Best, d.ChosenParent, d.SecP, globalState.S, ASN, d.L[ASN], d.BestRelation[ASN], globalState.W);

                        deltaU[ASN] += (notASN - d.U[ASN]);
                        
                    }
                }
            }
            int flipped = 0;
            for (int ASN = 0; ASN < deltaU.Length;ASN++ )
            {
                if (deltaU[ASN] > 0)//positive change in utility for this ASN
                {
                    flipped++;
                    if (globalState.S[ASN])
                        Console.WriteLine("AS: " + ASN + " is rolling back!!!");
                    globalState.S[ASN] = !globalState.S[ASN];
                    if (!quiet)
                    {
                        Console.WriteLine("AS: " + ASN + " had change in utility of " + deltaU[ASN]);
                    }
                }

            }
            stopwatch.Stop();
            Console.WriteLine("flip u took " + stopwatch.ElapsedMilliseconds + " ms");
            Console.WriteLine("it flipped " + flipped + " ASes");
            return true;
        }

        private bool computeHash(string command)
        {
            string[] pieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length < 3)
            {
                Console.WriteLine("usage: computehash <as1> <as2>");
                Console.WriteLine("please try again.");
                return false;
            }
            UInt32 as1, as2;
            if (!UInt32.TryParse(pieces[1], out as1) ||
                !UInt32.TryParse(pieces[2], out as2))
            {
                Console.WriteLine("Could not parse the AS numbers provided. Please try again.");
                return false;
            }


            UInt32 answer = ModifiedBfs.hash6432shift(as1, as2);
            Console.WriteLine("the hash of as1: " + as1 + " and as2: " + as2 + " is " + answer);
            return true;
        }

        private bool printBucketTable(ref List<Destination> ds,string command)
        {
            string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 2)
            {
                Console.WriteLine("usage: printbuckettable <d>");
                return false;
            }
            UInt32 dstnum;
            if (!UInt32.TryParse(cpieces[1], out dstnum))
            {
                Console.WriteLine("invalid d!");
                return false;
            }
            Destination d = new Destination();
            foreach (Destination curr in ds)
            {
                if (curr.destination == dstnum)
                    d = curr;
            }

            if (d.BucketTable == null)
                Console.WriteLine("null bucket table!");
            for (int i = 0; i < d.BucketTable.GetLength(0); i++)
            {
                for (int j = 0; j < d.BucketTable[i].GetLength(0); j++)
                {
                    if (d.BucketTable[i][ j] != null)
                    {
                        string towrite = "BucketTable [" + i + "," + j + "] is < ";
                        for (int k = 0; k < d.BucketTable[i][ j].Count; k++)
                        {
                            if (k < d.BucketTable[i][ j].Count - 1)
                                towrite = towrite + d.BucketTable[i][ j][k] + ", ";
                            else
                                towrite = towrite + d.BucketTable[i][j][k] + " > ";

                        }
                        Console.WriteLine(towrite);
                    }
                    else
                        Console.WriteLine("BucketTable ["+i+","+j+"] is null");
                }
            }
            return true;
        }

        private bool printSecP(ref List<Destination> ds, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("maximum AS #");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            string resp2 = "";
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 2)
                resp2 = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[2];

            int maxnum;
            int destNum;
            if (!int.TryParse(resp, out maxnum))
            {
                Console.WriteLine("invalid maximum.");
                return false;
            }
            if (!int.TryParse(resp2, out destNum))
            {
                Console.WriteLine("you did not specify a destination!");
                return false;
            }
            Destination d = new Destination();
            foreach (Destination curr in ds)
            {
                if (curr.destination == destNum)
                    d = curr;
            }
            if (d.BucketTable == null)
            {
                Console.WriteLine("destination "+destNum+" does not seem to be initialized!");
                return false;
            }
            Console.WriteLine("---------");
            Console.WriteLine("ASN :: SecP");
            for (int i = 1; i < d.SecP.Length && i < maxnum; i++)
            {
                Console.WriteLine(i + " :: " + d.SecP[i]);
            }
            Console.WriteLine("---------");
            return true;
        }

        private bool printState(ref bool[] S,string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("maximum AS #");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            int maxnum;
            if (!int.TryParse(resp, out maxnum))
            {
                Console.WriteLine("invalid maximum.");
                return false;
            }
            Console.WriteLine("---------");
            Console.WriteLine("ASN :: State");
            for (int i = 1; i < S.Length && i<maxnum; i++)
            {
                Console.WriteLine(i + " :: " + S[i]);
            }
            Console.WriteLine("---------");
            return true;
        }

        private bool printWeight(ref UInt16[] W, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("maximum AS #");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            int maxnum;
            if (!int.TryParse(resp, out maxnum))
            {
                Console.WriteLine("invalid maximum.");
                return false;
            }
            Console.WriteLine("---------");
            Console.WriteLine("ASN :: Weight");
            for (int i = 1; i < W.Length && i < maxnum; i++)
            {
                Console.WriteLine(i + " :: " + W[i]);
            }
            Console.WriteLine("---------");
            return true;
        }

        private bool getWorkerSecP(ref Worker w, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the SecP of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            int ASN;
            if (!int.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }

            Console.WriteLine("SecP of " + ASN + " in the worker is " + w.GetSecP(ASN));
            return true;
        }    

        private bool computeNotN(ref List<Destination> ds, ref GlobalState globalState, ref Worker w, string command)
        {
             string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 3)
            {
                Console.WriteLine("error: usage getpath <as#> <dstnum>");
                return false;
            }

            int dstNum;
            UInt32 ASN;
            if (!UInt32.TryParse(cpieces[1], out ASN) || !int.TryParse(cpieces[2],out dstNum))
            {
                Console.WriteLine("invalid ASN or destination.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == dstNum)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Reset();
                    stopwatch.Start();
                    int unotn = w.ComputeUtility(d.BucketTable, d.Best, d.ChosenParent, d.SecP, globalState.S, ASN, d.L[ASN], d.BestRelation[ASN], globalState.W);
                    stopwatch.Stop();

                    Console.WriteLine("Utility for " + ASN + " if they flip is: " + unotn + " computing this took " + stopwatch.ElapsedMilliseconds);
                    return true;
                }
            }
            Console.WriteLine("could not find destination.");
            return false;

            
        }

        private bool iterate(ref List<Destination> ds, ref GlobalState globalState, string command)
        {
            string resp="0";

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length >= 2)
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];


            UInt32 destNum=0;
            if (!UInt32.TryParse(resp, out destNum))
            {
                Console.WriteLine("Error invalid destination number.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == destNum)
                {
                    Console.WriteLine("running utility and updatepaths for destination: " + d.destination + " minrow: " + 0);
                    d.UpdatePaths(globalState.S);
                    d.ComputeU(globalState.W);

                    return true;
                }
            }
            Console.WriteLine("could not find destination.");
            return false;
        }

        private bool getSecP(ref List<Destination> ds, string command)
        {
             string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 3)
            {
                Console.WriteLine("error: usage getsecp <as#> <dstnum>");
                return false;
            }

            int dstNum;
            UInt32 ASN;
            if (!UInt32.TryParse(cpieces[1], out ASN) || !int.TryParse(cpieces[2],out dstNum))
            {
                Console.WriteLine("invalid ASN or destination.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == dstNum)
                {
                    Console.WriteLine("SecP of " + ASN + " in the BFS rooted at " + d.destination + " is " + d.GetSecP((int)ASN));
                    return true;
                }
            }
            Console.WriteLine("could not find destination.");
            return false;
        }

        private bool getUtility(ref List<Destination> ds, string command)
        {
             string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 3)
            {
                Console.WriteLine("error: usage getutility <as#> <dstnum>");
                return false;
            }

            int dstNum;
            UInt32 ASN;
            if (!UInt32.TryParse(cpieces[1], out ASN) || !int.TryParse(cpieces[2],out dstNum))
            {
                Console.WriteLine("invalid ASN or destination.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == dstNum)
                {
                    Console.WriteLine("Utility of " + ASN + " in the BFS rooted at " + d.destination + " is " + d.GetUtility((int)ASN));
                    return true;
                }
            }
            Console.WriteLine("could not find destination.");
            return false;

        }

        private bool getBest(ref List<Destination> ds, string command)
        {
                string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 3)
            {
                Console.WriteLine("error: usage getbest <as#> <dstnum>");
                return false;
            }

            int dstNum;
            int ASN;
            if (!int.TryParse(cpieces[1], out ASN) || !int.TryParse(cpieces[2],out dstNum))
            {
                Console.WriteLine("invalid ASN or destination.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == dstNum)
                {

                    Console.WriteLine("Best[i] of " + ASN + " in the BFS rooted at " + d.destination + " is " + d.GetBest(ASN));
                    return true;
                }
            }
            Console.WriteLine("could not find destination.");
            return false;
        }

        private bool getPath(ref List<Destination> ds, string command)
        {
               string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 3)
            {
                Console.WriteLine("error: usage getpath <as#> <dstnum>");
                return false;
            }

            int dstNum;
            UInt32 ASN;
            if (!UInt32.TryParse(cpieces[1], out ASN) || !int.TryParse(cpieces[2],out dstNum))
            {
                Console.WriteLine("invalid ASN or destination.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == dstNum)
                {

                    Console.WriteLine("Path from " + ASN + " to " + d.destination + " is " + d.GetPath(ASN));
                    return true;
                }
            }
            Console.WriteLine("could not find destination");
            return false;

            
        }

        private bool getWorkerPath(ref Worker w, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the path of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            UInt32 ASN;
            if (!UInt32.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }

            Console.WriteLine("Path from " + ASN + " in the worker is " + w.GetPath(ASN));
            return true;
        }

        private bool getS(ref bool[] S, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the State of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            int ASN;
            if (!int.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }
            if (ASN < S.Length)
            {
                Console.WriteLine("State of " + ASN + " is " + S[ASN]);
                return true;
            }
            Console.WriteLine("AS was too large.");
            return false;
        }

        private bool setS(ref bool[] S, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to flip the State of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            int ASN;
            if (!int.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }

            S[ASN] = !S[ASN];
            Console.WriteLine("State of " + ASN + " is now " + S[ASN]);
            return true;
        }

        private bool setW(ref UInt16[] W, string command)
        {
            string strASN,strW;
            strASN = strW = "";

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length <3)
            {
                Console.WriteLine("Usage: setW <ASN> <Weight>");
           
            }
            else
            {
                strASN = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                strW = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[2];
            }
            int ASN;
            UInt16 newW;
            if (!int.TryParse(strASN, out ASN) || !UInt16.TryParse(strW, out newW))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }

            W[ASN] = newW;
            Console.WriteLine("Weight of " + ASN + " is now " + W[ASN]);
            return true;
        }

        private bool getW(ref UInt16[] W, string command)
        {
            string strASN, strW;
            strASN = strW = "";

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Usage: getW <ASN> ");

            }
            else
            {
                strASN = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
                
            }
            int ASN;
            
            if (!int.TryParse(strASN, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }

           
            Console.WriteLine("Weight of " + ASN + " is  " + W[ASN]);
            return true;
        }

        private bool getWorkerParent(ref Worker w, string command)
        {
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a node to get the parent of");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            int ASN;
            if (!int.TryParse(resp, out ASN))
            {
                Console.WriteLine("invalid ASN.");
                return false;
            }

            Console.WriteLine("Parent of " + ASN + " in the worker is " + w.GetParent(ASN));
            return true;
        }

        private bool getL(ref List<Destination> ds,string command)
        {
         
            string[] cpieces = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (cpieces.Length < 3)
            {
                Console.WriteLine("error: usage getl <as#> <dstnum>");
                return false;
            }

            int dstNum;
            int ASN;
            if (!int.TryParse(cpieces[1], out ASN) || !int.TryParse(cpieces[2],out dstNum))
            {
                Console.WriteLine("invalid ASN or destination.");
                return false;
            }

            foreach (Destination d in ds)
            {
                if (d.destination == dstNum)
                {
                    Console.WriteLine("Level of " + ASN + " in the BFS rooted at " + d.destination + " is " + d.getL(ASN));
                    return true;
                }
            }
            Console.WriteLine("could not find destination. is it initialized?");
            return false;
        }

        private bool initS(ref bool[] S,string command)
        {
            string resp;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Initializing S. Please enter a string of 1's and 0's with no spaces to manually initialize the state (starting at ASN 1)" +
               " or a number between 0 and 100 preceded by 'p' to set a random percentage of S to be secure.");
                resp = Console.ReadLine();
            }
            else
            {
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            }

            //clear S
            for (int i = 0; i < S.Length; i++)
                S[i] = false;

            if (resp[0] == 'p')
            {
                //setting a random percentage
                int perc;
                if (!int.TryParse(resp.Substring(1), out perc))
                {
                    Console.Write("could not parse percentage!");
                    return false;
                }
                if (perc > 0 && perc < 100)
                {
                    Random myRand = new Random();
                    for (int i = 0; i < S.Length; i++)
                    {
                        if (myRand.Next(100) < perc)
                        {
                            S[i] = true;
                        }
                    }
                }
                else if (perc == 100)
                {
                    for (int i = 0; i < S.Length; i++)
                        S[i] = true;
                }
                else
                {
                    for (int i = 0; i < S.Length; i++)
                        S[i] = false;

                }

                Console.WriteLine("done setting " + perc + "% of S to be true");
                return true;

            }
            else
            {
                //manually setting it.
                for (int i = 0; i < resp.Length; i++)
                {
                    if (i < S.Length)
                    {
                        if (resp[i] == '1')
                            S[i] = true;
                        else
                            S[i] = false;
                    }
                }
                Console.WriteLine("done setting S manually. ");
                return true;
            }

           
        }

        private bool initDestination(ref NetworkGraph g, ref Destination d,string command)
        {
            string resp;
            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                Console.WriteLine("Please enter a destination ASN: ");
                resp = Console.ReadLine();

    
            }
            else
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];

            UInt32 destNum;
            if (!UInt32.TryParse(resp, out destNum))
            {
                Console.WriteLine("invalid ASN! ");
                return false;
            }
            if (g.GetNode(destNum) == null)
            {
                Console.WriteLine("could not retrieve destination " + d + " from the graph.");
                return false;
            }

            Console.WriteLine("initializing variables and running RTA");
            MiniDestination miniDest = SimulatorLibrary.initMiniDestination(g, destNum, false);
             d = new Destination(miniDest);
             bool[] tempS = new bool[Constants._numASNs];
             for (int i = 0; i < tempS.Length;i++)
                 tempS[i] = false; ;
             d.UpdatePaths(tempS);//init paths with S = all false
             Console.WriteLine("done initializing. Current active destination is: " + destNum);
            return true;
        }

        private bool initGraph(ref NetworkGraph g,string command)
        {
            g = new NetworkGraph();
            string resp;

            if (command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length > 1)
                resp = command.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];
            else
            {
                Console.WriteLine("Please enter a file name or \"testgraph\" to use the graph from the routing tree algorithm figure in the Sigcomm 2010 paper");
                resp = Console.ReadLine().ToLower();
            }
            if (resp == "testgraph")
            {
                Console.WriteLine("loading test graph ...");
                g = getTestGraph();
                Console.WriteLine("done.");
                return true;
            }
            else
            {
                if (File.Exists(resp))
                {
                    InputFileReader iFR = new InputFileReader(resp, g);
                    iFR.ProcessFile();
                    Int32 p2pEdges = 0;
                    Int32 c2pEdges = 0;
                    foreach(var ASNode in g.GetAllNodes())
                    {
                        p2pEdges += ASNode.GetNeighborTypeCount(RelationshipType.PeerOf);
                        c2pEdges += ASNode.GetNeighborTypeCount(RelationshipType.CustomerOf);
                        c2pEdges += ASNode.GetNeighborTypeCount(RelationshipType.ProviderTo);
                    }
                    Console.WriteLine("read in the graph it has " + g.NodeCount + " nodes and " + g.EdgeCount + " edges");
                    Console.WriteLine("P2P: " + p2pEdges + " C2P: " + c2pEdges);
                    return true;
                }
                else
                {
                    Console.WriteLine("that file does not exist. Going back to main menu now.");
                    return false;
                }
            }

        }

        private void help()
        {
            Console.WriteLine("---------");
            Console.WriteLine("How to use this interface: (note: none of these commands are case sensitive)");
            Console.WriteLine("---------");
            Console.WriteLine();
            Console.WriteLine("Input [filename] - enables you to enter a filename or use the test graph. You need to do this to set up the graph");
            Console.WriteLine();
            Console.WriteLine("Destination [<ASN>|all] - sets a node (ASN) in the graph as the destination (or all ASes if 'all' is the parameter) we are considering");
            Console.WriteLine();
            Console.WriteLine("SetState <p30/110101> - can either enter p followed by a number between 0 and 100 (no spaces) to randomly turn on a specific percent of the nodes" +
                " or a binary string of 0's and 1's to manually set the values in S");
            Console.WriteLine();
            Console.WriteLine("initglobalstate :early adopters:top x:top x% - run the command with no parameters for more info");
            Console.WriteLine();
            Console.WriteLine("GetL <ASN> - get the level of the ASN in the tree rooted at d.");
            Console.WriteLine();
            Console.WriteLine("GetUtility <ASN> - get utility of specified node");
            Console.WriteLine();
            Console.WriteLine("GetBest <ASN> - get the Best[i] set of the ASN in the tree rooted at d");
            Console.WriteLine();
            Console.WriteLine("GetSecP <ASN> - get whether this node has a secure path to the destination");
            Console.WriteLine();
            Console.WriteLine("GetPath <ASN> - get path from this ASN to the destination.");
            Console.WriteLine();
            Console.WriteLine("sets <ASN> - flip S[ASN]");
            Console.WriteLine();
            Console.WriteLine("gets <ASN> - get S[ASN]");
            Console.WriteLine();
            Console.WriteLine("getaveragebest [destination] - get the average length of the best vectors for nodes to this destination AS");
            Console.WriteLine();
            Console.WriteLine("setutilitycomputation [basic|incoming|outgoing] - sets utility computation function.");
            Console.WriteLine("getnonstubs - show us the non-stubs in the graph");
            Console.WriteLine();
            Console.WriteLine("getstubs - show us the stubs in the graph");
            Console.WriteLine();
            Console.WriteLine("wgetpath - get the path for this node as computed by the worker");
            Console.WriteLine();
            Console.WriteLine("wgetparent - get the parent for this node as computed by the worker");
            Console.WriteLine();
            Console.WriteLine("wgetsecp - get secP for this node as computed by the worker");
            Console.WriteLine();
            Console.WriteLine("nodeswithnopath [ASN] - how many nodes in the graph have no path to d");
            Console.WriteLine();
            Console.WriteLine("runtest - runs the full benchmark of tests, outputs a pile of data to stdout");
            Console.WriteLine();
            Console.WriteLine("Iterate - this will run UpdatePaths and ComputeU for this destination using the current value of S");
            Console.WriteLine();
            Console.WriteLine("iterateall - runs the iterate function from simulator library for each node");
            Console.WriteLine();
            Console.WriteLine("not <ASN> - tell us the utility for this ASN if they flip their state; this must be called before invoking worker methods");
            Console.WriteLine();
            Console.WriteLine("compareu [maxlines] [benefit]- outputs a comparison of utility for all ASes before/after flipping. It takes 2 optional parameters, the maximum ASN to go up to and whether to only print nodes that benefit");
            Console.WriteLine();
            Console.WriteLine("flipu [quiet?]- does the same computation as compareu, but flips nodes if they benefit from changing. Give it a parameter to make it quietly flip states and not print them all.");
            Console.WriteLine();
            Console.WriteLine("flipallu [quiet?] - same as flipu but for all destinations");
            Console.WriteLine();
            Console.WriteLine("numberon - tell us how many ASNs are on");
            Console.WriteLine();
            Console.WriteLine("clear - clear all data structures");
            Console.WriteLine();
            Console.WriteLine("turnonstubs - turns on all stubs");
            Console.WriteLine();
            Console.WriteLine("(not)onlynonstubs - build BFS only considering nonstubs or using all nodes (notonlynonstubs)");
            Console.WriteLine();
            Console.WriteLine("getcustomers [ASN] [stubs] - get customers of this ASN, extra parameter if you only want stubs");
            Console.WriteLine();
            Console.WriteLine("getproviders [ASN] [stubs] - get providersof this ASN, extra parameter if you only want stubs");
            Console.WriteLine();
            Console.WriteLine("getpeers [ASN] [stubs] - get peers of this ASN, extra parameter if you only want stubs");
            Console.WriteLine();
            Console.WriteLine("printbuckettable - prints the entire buckettable (verbose)");
            Console.WriteLine();
            Console.WriteLine("printstate [maxASN] - prints the value of S for each node up to maxASN ");
            Console.WriteLine();
            Console.WriteLine("printsecp [maxASN] - prints the value of SecP for each node up to maxASN ");
            Console.WriteLine();
            Console.WriteLine("List - print all files in current directory");
            Console.WriteLine();
            Console.WriteLine("Exit - quit");

        }
    }
}
