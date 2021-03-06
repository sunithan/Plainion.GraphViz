﻿using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.CodeInspection.Packaging
{
    [Export( typeof( PackagingGraphMenuItem ) )]
    public partial class PackagingGraphMenuItem : MenuItem
    {
        [ImportingConstructor]
        public PackagingGraphMenuItem( PackagingGraphMenuItemModel model )
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
