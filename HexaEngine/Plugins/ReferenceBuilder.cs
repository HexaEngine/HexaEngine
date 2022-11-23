namespace HexaEngine.Plugins
{
    using HexaEngine.Core;
    using HexaEngine.Core.Unsafes;
    using System.Collections.Generic;

    public class ReferenceBuilder
    {
        public unsafe static void Sort(Plugin** plugins, uint nPlugins, Plugin*** pppPlugin)
        {
            HashSet<Pointer<Plugin>> pluginsHashSet = new();
            HashSet<Tuple<Pointer<Plugin>, Pointer<Plugin>>> edges = new();
            for (uint i = 0; i < nPlugins; i++)
            {
                var pPlugin = plugins[i];
                for (uint j = 0; j < pPlugin->Header->DependencyCount; j++)
                {
                    Plugin* dep;
                    Find(plugins, nPlugins, pPlugin->Header->Dependencies[j], &dep);
                    edges.Add(new((Pointer<Plugin>)dep, (Pointer<Plugin>)pPlugin));
                }
                pluginsHashSet.Add(pPlugin);
            }

            var pluginsSorted = TopologicalSort(pluginsHashSet, edges);

            Plugin** ppPluginsSorted = Utilities.Alloc<Plugin>(pluginsSorted.Count);
            *pppPlugin = ppPluginsSorted;
            for (int i = 0; i < pluginsSorted.Count; i++)
            {
                ppPluginsSorted[i] = pluginsSorted[i];
            }
        }

        static List<T> TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements
            var L = new List<T>();

            // Set of all nodes with no incoming edges
            var S = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            // while S is non-empty do
            while (S.Any())
            {

                //  remove a node n from S
                var n = S.First();
                S.Remove(n);

                // add n to tail of L
                L.Add(n);

                // for each node m with an edge e from n to m do
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph
                    edges.Remove(e);

                    // if m has no other incoming edges then
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        // insert m into S
                        S.Add(m);
                    }
                }
            }

            // if graph has edges then
            if (edges.Any())
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                // return L (a topologically sorted order)
                return L;
            }
        }

        public unsafe static void Find(Plugin** plugins, uint nPlugins, char* name, Plugin** ppPlugin)
        {
            for (uint i = 0; i < nPlugins; i++)
            {
                var pPlugin = plugins[i];

                if (Utilities.StrCmp(name, pPlugin->Header->Name))
                {
                    *ppPlugin = pPlugin;
                }
            }
        }

        public unsafe static void Merge(Plugin** plugins, uint nPlugins, Record*** pppRecords, uint* nRecords)
        {
            Dictionary<ulong, Pointer<Record>> dict = new();
            for (uint i = 0; i < nPlugins; i++)
            {
                Plugin* plugin = plugins[i];
                for (uint j = 0; j < plugin->Header->RecordCount; j++)
                {
                    Record* record = plugin->Records[j];
                    ulong key = record->Header->Id;
                    if (dict.ContainsKey(key))
                    {
                        dict[key] = record;
                    }
                    else
                    {
                        dict.Add(record->Header->Id, record);
                    }
                }
            }

            Record** ppRecord = Utilities.Alloc<Record>(dict.Count);
            *pppRecords = ppRecord;
            *nRecords = (uint)dict.Count;

            var result = dict.Select(x => x.Value).ToArray();

            for (uint i = 0; i < *nRecords; i++)
            {

            }
            
        }

        public unsafe static void Build(Plugin** plugins)
        {

        }
    }
}