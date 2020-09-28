using System.Linq;
using Marten.Testing.Documents;
using Marten.Testing.Harness;
using Shouldly;
using Xunit;

namespace Marten.Testing.Bugs
{

    public class Bug_337_certain_boolean_searches_are_not_using_searchable_field: BugIntegrationContext
    {
        [Fact]
        public void use_searchable_fields_in_generated_sql()
        {
            StoreOptions(_ =>
            {
                _.Schema.For<Target>().Duplicate(x => x.Flag).GinIndexJsonData();
            });

            using (var session = theStore.OpenSession())
            {
                var cmd1 = session.Query<Target>().Where(x => x.Flag == false).ToCommand();

                var cmd2 = session.Query<Target>().Where(x => !x.Flag).ToCommand();

                cmd1.CommandText.ShouldBe($"select d.id, d.data, d.mt_version from {SchemaName}.mt_doc_target as d where d.flag = :p0");
                cmd2.CommandText.ShouldBe($"select d.id, d.data, d.mt_version from {SchemaName}.mt_doc_target as d where (d.flag is null or d.flag = False)");
            }
        }

        [Fact]
        public void booleans_in_generated_sql_without_being_searchable()
        {
            StoreOptions(_ =>
            {
                _.Schema.For<Target>().GinIndexJsonData();
                //_.Schema.For<Target>().Duplicate(x => x.Flag);
            });

            using (var session = theStore.OpenSession())
            {
                var cmd1 = session.Query<Target>().Where(x => x.Flag == false).ToCommand();

                cmd1.CommandText.ShouldBe("select d.data, d.id, d.mt_version, d.mt_last_modified, d.mt_dotnet_type from public.mt_doc_target as d where d.data @> :arg0");
            }
        }
    }
}
