﻿using Plainion.GraphViz.Model;

namespace Plainion.GraphViz.Presentation
{
    /// <summary>
    /// Null-object pattern for <see cref="IGraphPicking"/>.
    /// </summary>
    public class NullGraphPicking : IGraphPicking
    {
        public bool Pick(Node node)
        {
            return true;
        }

        public bool Pick(Edge edge)
        {
            return true;
        }
    }
}
