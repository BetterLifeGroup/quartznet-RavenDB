using System;
using System.Threading;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace Quartz.Impl.RavenDB
{
    public class DocumentStoreHolder
    {
        private static readonly Lazy<IDocumentStore> store = new Lazy<IDocumentStore>(CreateStore);

        public static IDocumentStore Store => store.Value;

        private static IDocumentStore CreateStore()
        {
            var documentStore = new DocumentStore()
            {
                Urls = RavenJobStore.Urls,
                Database = RavenJobStore.Database,
                Certificate = RavenJobStore.Certificate
            };


            // For multithreaded debugging need to uncomment next line (prints thread id and stack trace)
            //documentStore.OnBeforeStore += (sender, args) =>
            //{
            //    Console.WriteLine("PUT of " + args.DocumentId + " on " + Thread.CurrentThread.ManagedThreadId);
            //    Console.WriteLine("Stack trace:" + Environment.StackTrace);
            //};

            documentStore.Initialize();

            //create db if it doesn't exist
            var rec = documentStore.Maintenance.Server.Send(new GetDatabaseRecordOperation(RavenJobStore.Database));
            if (rec == null)
            {
                //does not exist
                documentStore.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(RavenJobStore.Database)
                {
                    Disabled = false
                }));
            }

            documentStore.OnBeforeQuery += (sender, beforeQueryExecutedArgs) =>
            {
                beforeQueryExecutedArgs.QueryCustomization.WaitForNonStaleResults();
            };
            return documentStore;
        }
    }
}
