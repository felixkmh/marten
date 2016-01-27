﻿using Marten.Services;
using Marten.Testing.Documents;
using Xunit;

namespace Marten.Testing.Session.RequestCounter
{
    public class document_session_with_request_counter_threshold_Tests
    {
        [Fact]
        public void should_invoke_defined_action_if_threshold_is_exceeded()
        {
            bool wasInvoked = false;

            var documentStore = DocumentStore.For(opt =>
            {
                opt.AutoCreateSchemaObjects = true;
                opt.Connection(ConnectionSource.ConnectionString);
                opt.RequestCounterThreshold = new RequestCounterThreshold(1, () => wasInvoked = true);
            });

            using (var session = documentStore.OpenSession())
            {
                session.Store(new User {FirstName = "Jens"});
                session.SaveChanges();

                session.Store(new User { FirstName = "Ida"});
                session.SaveChanges(); //this is the second request, should invoke threshold
            }

            wasInvoked.ShouldBeTrue();
        }    
    }
}