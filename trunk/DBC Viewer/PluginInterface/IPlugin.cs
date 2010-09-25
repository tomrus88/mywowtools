using System.Data;

namespace PluginInterface
{
    public interface IPlugin
    {
        void Run(DataTable data);
    }
}
