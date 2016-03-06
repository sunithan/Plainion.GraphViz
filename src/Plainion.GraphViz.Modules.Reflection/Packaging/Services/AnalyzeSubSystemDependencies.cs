﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Plainion.GraphViz.Modules.Reflection.Packaging;
using Plainion.GraphViz.Modules.Reflection.Packaging.Spec;
using Plainion.GraphViz.Presentation;

namespace Plainion.GraphViz.Modules.Reflection.Packaging.Services
{
    class AnalyzeSubSystemDependencies : AnalyzeBase
    {
        private Package myPackage;
        private List<Type> myTypes;

        public string PackageName { get; set; }

        protected override void Load()
        {
            myPackage = Config.Packages.Single( p => p.Name.Equals( PackageName, StringComparison.OrdinalIgnoreCase ) );

            myTypes = Load( myPackage )
                .SelectMany( asm => asm.GetTypes() )
                .ToList();
        }

        protected override Task<Tuple<Type, Type>[]>[] Analyze()
        {
            return new[]
            {
                Task.Run<Tuple<Type, Type>[]>(() =>
                {
                    var tasks = myTypes
                        .Select(t => Task.Run<IEnumerable<Tuple<Type, Type>>>(() => Analyze(t)))
                        .ToArray();

                    Task.WaitAll(tasks);

                    return tasks.SelectMany(t => t.Result).ToArray();
                })
            };
        }

        private IEnumerable<Tuple<Type, Type>> Analyze( Type type )
        {
            Console.Write( "." );

            var cluster = myPackage.Clusters.FirstOrDefault( c => c.Matches( type.FullName ) );

            return new Reflector( AssemblyLoader, type ).GetUsedTypes()
                .Where( myTypes.Contains )
                .Where( t => type != t )
                .Where( t => cluster == null || cluster != myPackage.Clusters.FirstOrDefault( c => c.Matches( t.FullName ) ) )
                .Select( usedType => GraphUtils.Edge( type, usedType ) );
        }

        protected override AnalysisDocument GenerateDocument( IReadOnlyCollection<Tuple<Type, Type>> edges )
        {
            var doc = new AnalysisDocument();

            foreach( var node in myTypes.Select( GraphUtils.Node ).Distinct() )
            {
                if( !edges.Any( e => e.Item1 == node || e.Item2 == node ) )
                {
                    continue;
                }

                doc.AddNode( node.FullName );
                doc.Captions.Add( new Caption( node.FullName, node.Name ) );

                // in case multiple cluster match we just take the first one
                var matchedCluster = myPackage.Clusters.FirstOrDefault( c => c.Matches( node.FullName ) );
                if( matchedCluster != null )
                {
                    doc.AddToCluster( matchedCluster.Name, node.FullName );
                }
            }

            foreach( var edge in edges )
            {
                doc.AddEdge( edge.Item1.FullName, edge.Item2.FullName );
            }

            return doc;
        }
    }
}
