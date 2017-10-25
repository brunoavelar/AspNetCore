using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CityInfo.Api;

namespace CityInfo.Test
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration)
        {
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public override void ConfigureServices(IServiceCollection services)
        {
            //Configuration["connectionStrings:cityInfoDBConnectionString"] = "aaa";
            base.ConfigureServices(services);
        }
    }
}
