using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecureSimulator;
using System.IO;

namespace TestingApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            string testPath = "mycontainer";
            string directoryName = "";
            //   string newpth=SimulatorLibrary.generateDirectoryName(testPath, ref  directoryName);
           TestingClass test = new TestingClass();
      //      SimulatorLibrary.setHash(false);
       //     SimulatorLibrary.useMiniDWeight = true;
        //    SimulatorLibrary.useLessHarshWeight = true;
      //      testShortestPaths tsp = new testShortestPaths();
            // tsp.testSPInterface();

            /*     string[] topologies ={"Cyclops_20101209_IXP_AFRINIC_v2.txt",
                               "Cyclops_20101209_IXP_APNIC_v2.txt",
                               "Cyclops_20101209_IXP_ARIN_v2.txt",
                               "Cyclops_20101209_IXP_LACNIC_v2.txt",
                               "Cyclops_20101209_IXP_RIPE_v2.txt",
                               "Cyclops_DHYB80_70.txt",
                               "Cyclops_DHYB80_50.txt",
                               "Cyclops_DHYB80_25.txt",
                                      "CyclopsIXP20101209-Big5-nochildren.txt"};
                 string topoDir = "C:\\Users\\phillipa\\Desktop\\adoptTraffic\\AugmentedGraphs\\";
                 foreach (string t in topologies)
                     SimulatorLibrary.writeNodeListsToFile(topoDir, t);*/

            /* temporary code to look at traffic to stubs through 
             * secure providers */
          //  ResultsExplorer res = new ResultsExplorer();
           // string[] simulationDirectories = Directory.GetDirectories(ResultsExplorer.defaultResultsDirectory);
          //  string directory = simulationDirectories[33];

      //      resultObject canonical = res.loadSimulationNonInteractive(directory);
       
       //     TrafficEngineering.trafficThroughSecureProviders(canonical);

        //    Console.WriteLine("foob");

            //uncomment to this to get the regular program.
            test.CLI(false);

        }

    }
}
