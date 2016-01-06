// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UviContextMenuStrip.cs" company="DigitalGlobe">
//   Copyright 2015 DigitalGlobe
//   
//      Licensed under the Apache License, Version 2.0 (the "License");
//      you may not use this file except in compliance with the License.
//      You may obtain a copy of the License at
//   
//          http://www.apache.org/licenses/LICENSE-2.0
//   
//      Unless required by applicable law or agreed to in writing, software
//      distributed under the License is distributed on an "AS IS" BASIS,
//      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//      See the License for the specific language governing permissions and
//      limitations under the License.
// </copyright>
// <summary>
//   The UVI context menu strip.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Gbdx.Vector_Index.Forms
{
    using System.Windows.Forms;

    /// <summary>
    /// The UVI context menu strip.
    /// </summary>
    public class UviContextMenuStrip : ContextMenuStrip
    {
        /// <summary>
        /// The attached node.
        /// </summary>
        private TreeNode attachedNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="UviContextMenuStrip"/> class.
        /// </summary>
        /// <param name="parent">
        /// The parent.
        /// </param>
        public UviContextMenuStrip(TreeNode parent)
        {
            this.attachedNode = parent;
        }

        /// <summary>
        /// Gets or sets the attached tree node.
        /// </summary>
        public TreeNode AttachedTreeNode
        {
            get
            {
                return this.attachedNode;
            }

            set
            {
                this.attachedNode = value;
            }
        }
    }
}