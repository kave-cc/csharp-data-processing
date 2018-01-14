/*
 * Copyright 2014 Technische Universität Darmstadt
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.IO;
using System.Threading;
using KaVE.FeedbackProcessor.CompletionEventStatistics;
using KaVE.FeedbackProcessor.DebuggingHacks;
using KaVE.FeedbackProcessor.EditLocation;
using KaVE.FeedbackProcessor.Naming;
using KaVE.FeedbackProcessor.Preprocessing;
using KaVE.FeedbackProcessor.Preprocessing.Model;
using KaVE.FeedbackProcessor.SanityChecks;
using KaVE.FeedbackProcessor.StatisticsForPapers;
using KaVE.FeedbackProcessor.StatisticsUltimate;
using KaVE.FeedbackProcessor.UserStatistics;
using KaVE.FeedbackProcessor.WatchdogExports;
using KaVE.RS.SolutionAnalysis;

namespace KaVE.FeedbackProcessor
{
    internal class FeedbackProcessorApp
    {
        private static readonly int NumWorkers = Environment.ProcessorCount;

        private const string Root = @"C:\Users\Sebastian\Desktop\kave-data-dir\";
        //private const string Root = @"E:\";
        //private const string Root = @"C:\Users\seb2\Desktop\interval-tests\";

        private const string DirTmp = Root + @"Tmp\";
        //private const string DirTmp = @"E:\Tmp\";

        private const string DirEventsIn = Root + @"Events-raw\";

        private const string DirEventsOut = Root + @"Events\";
        //private const string DirEventsIn = Root + @"Events-some\";
        //private const string DirEventsOut = Root + @"Events-some-out\";

        private const string WdFolder = Root + @"watchdog\";
        private const string SvgFolder = Root + @"svg\";

        private const string DirContexts = Root + @"Contexts\";

        public static void Main()
        {
            var startedAt = DateTime.Now;
            Console.WriteLine(@"started at: {0}", startedAt);
            Console.WriteLine(
                @"Running a {0}bit process with {1} CPUs.",
                Environment.Is64BitProcess ? 64 : 32,
                NumWorkers);

            RunMemExampleToEnsureSupportForBigObjects();

            //new SanityCheckApp().Run(); 
            //new TimeBudgetEvaluationApp(Logger).Run();
            //new SSTSequenceExtractor(Logger).Run();
            //RunExhaustiveNamesFixTests();
            //RunPreprocessing();
            //RunWatchdogExport();
            //RunInteractionStatistics();
            RunSmokeTest();
            //RunContextStatistics();
            //RunEditLocationAnalysis();
            //RunSSTTransformationComparison(Root + @"Contexts-161031", Root + @"Contexts-170428");
            //RunEmDebug();
            // RunUserStats(); // used to export positions used in the Demographic generator on Java

            var endedAt = DateTime.Now;
            Console.WriteLine(@"ended at {0}, took {1}", endedAt, (endedAt - startedAt));
            Console.ReadKey();
        }

        private static void RunEmDebug()
        {
            var io = new PreprocessingIo(DirContexts, DirTmp, DirTmp);
            new EmDebug(io, new ContextStatisticsLogger(), NumWorkers).Run();
        }

        private static void RunEditLocationAnalysis()
        {
            var io = new PreprocessingIo(DirEventsOut, DirTmp, DirTmp);
            new EditLocationAnalysisRunner(NumWorkers, io, new EditLocationAnalysisLogger()).Run();
        }

        private static void RunSSTTransformationComparison(string oldContexts, string newContexts)
        {
            var oldIo = new PreprocessingIo(oldContexts, DirTmp, DirTmp);
            var newIo = new PreprocessingIo(newContexts, DirTmp, DirTmp);
            new SSTTransformationComparison(oldIo, newIo).Run();
        }

        private static void RunInteractionStatistics()
        {
            var io = new PreprocessingIo(DirEventsOut, DirTmp, DirTmp);
            new InteractionStatisticsRunner(io, new InteractionStatisticsLogger(), NumWorkers).Run();
        }

        private static void RunContextStatistics()
        {
            var io = new PreprocessingIo(DirContexts, DirTmp, DirTmp);
            var cf = new ContextFilter(GeneratedCode.Include, Duplication.Include);
            new ContextStatisticsRunner(io, cf, new ContextStatisticsLogger(), NumWorkers).Run();
        }

        private static void RunMemExampleToEnsureSupportForBigObjects()
        {
            // ReSharper disable once UnusedVariable
            var arr = new long[540000000];
        }

        private static void RunExhaustiveNamesFixTests()
        {
            new NameFixesIntegrationTest(NumWorkers, DirContexts).TryToNameFixSomeNamesFromContexts();
        }

        private static void RunPreprocessing()
        {
            CleanDirs(DirTmp, DirEventsOut);
            new PreprocessingRunner(DirEventsIn, DirTmp, DirEventsOut, NumWorkers).Run();
        }

        private static void RunSmokeTest()
        {
            new ReadAllEventsAndCalcHashCodeRunner(DirEventsOut, NumWorkers).Run();
        }

        private static void RunWatchdogExport()
        {
            CleanDirs(WdFolder, SvgFolder);
            //new WatchdogExportRunner(DirEventsOut, WdFolder, SvgFolder).RunDebugging();
            new WatchdogExportRunner(DirEventsOut, WdFolder, SvgFolder).RunTransformation();
        }

        private static void RunCompletionEventStatistics()
        {
            var io = new CompletionEventStatisticsIo(DirEventsOut);
            var log = new CompletionEventStatisticsLogger();
            new CompletionEventStatisticsRunner(io, log).Run();
        }

        private static void RunUserStats()
        {
            new UserStatsRunner(new UserStatsIo(DirEventsOut, "")).Run();
        }

        private static void RunStatisticsForPaperCreation()
        {
            var printer = new StatisticsPrinter();
            var io = new StatisticsIo(null, null); //DirEventsCompletion_KeepNoTrigger, DirEventsAll);
            new StatisticsForPaperRunner(io, printer).Run();
        }

        private static void RunApiStatisticsRunner()
        {
            new ApiStatisticsRunner().Run(DirContexts);
            new LocCounter().Run(DirContexts);
        }

        private static void CleanDirs(params string[] dirs)
        {
            foreach (var dir in dirs)
            {
                Console.WriteLine(@"cleaning: {0}", dir);
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                    while (Directory.Exists(dir))
                    {
                        Thread.Sleep(250);
                    }
                }
                Directory.CreateDirectory(dir);
            }
        }
    }
}