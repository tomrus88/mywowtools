using System.Data;

namespace PluginInterface
{
    public interface IPlugin
    {
        string Name { get; }
        void Run(DataTable data);
    }
}
