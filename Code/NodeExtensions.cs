// (c) 2026 Else Hell Entertainment
// License: MIT License (see LICENSE in project root for details)
// Author(s): Miska Rihu <miska.rihu@tuni.fi>

using System.Collections.Generic;
using Godot;

namespace EHE.Common.Godot.Extensions
{
    /// <summary>
    /// Includes extension methods for Godot <see cref="Node"/>.
    /// </summary>
    public static class NodeExtensions
    {
        // Documentation generated with GitHub Copilot.
        /// <summary>
        ///  Finds and returns the first child of the specified type
        ///  <typeparamref name="TNode"/>.
        /// </summary>
        ///
        /// <typeparam name="TNode">
        ///  The type of <see cref="Node"/> to search for. Must inherit from
        ///  <see cref="Node"/>.
        /// </typeparam>
        /// <param name="node">
        ///  The calling node; the search begins from this node's immediate
        ///  children.
        /// </param>
        /// <param name="recurse">
        ///  If <c>true</c>, the search will recurse into each child node
        ///  (depth-first). If <c>false</c>, only the immediate children are
        ///  checked.
        /// </param>
        ///
        /// <returns>
        ///  The first child node that is of type <typeparamref name="TNode"/>,
        ///  or <c>null</c> if no matching node is found.
        /// </returns>
        ///
        /// <remarks>
        ///  The search order is depth-first: each child is checked in
        ///  enumeration order; when recursion is enabled, the method
        ///  immediately searches the child's subtree before continuing to the
        ///  next sibling.
        /// </remarks>
        public static TNode GetFirstChildOfType<TNode>(this Node node, bool recurse = false)
            where TNode : Node
        {
            foreach (var child in node.GetChildren())
            {
                if (child is TNode wantedChild)
                {
                    return wantedChild;
                }

                // Recursion disabled or nothing to recurse into.
                if (!recurse || child.GetChildCount() == 0)
                {
                    continue;
                }

                var recursionResult = child.GetFirstChildOfType<TNode>(true);

                if (recursionResult != null)
                {
                    return recursionResult;
                }
            }

            return null;
        }

        // Documentation generated with GitHub Copilot.
        /// <summary>
        ///  Finds all immediate child nodes of the specified
        ///  <typeparamref name="TNode"/> type and returns them as a
        ///  <see cref="List{T}"/>. Optionally the search can recurse into
        ///  child nodes, and you can control whether to descend into nodes
        ///  that already matched the requested type.
        /// </summary>
        ///
        /// <typeparam name="TNode">
        ///  The type of <see cref="Node"/> to search for. Use a Godot node
        ///  type (for example: <see cref="Sprite3D"/>, <see cref="Node3D"/>,
        ///  or your custom node types).
        /// </typeparam>
        /// <param name="node">
        ///  The calling node; the search begins from this node's immediate
        ///  children.
        /// </param>
        /// <param name="recurse">
        ///  If <c>true</c>, the search will recurse into each child node
        ///  (depth-first). If <c>false</c>, only the immediate children are
        ///  checked.
        /// </param>
        /// <param name="recurseMatching">
        ///  When <c>true</c>, nodes that match <typeparamref name="TNode"/>
        ///  will also be recursed into (so their children may produce
        ///  additional matches). When <c>false</c>, matching nodes are added
        ///  to the results but their subtrees are not traversed.
        /// </param>
        ///
        /// <returns>
        ///  A list containing all matching child nodes or <c>null</c> if none
        ///  are found.
        /// </returns>
        ///
        /// <remarks>
        ///  The order of nodes in the returned list is depth-first: a matched
        ///  child is added when encountered, and recursion into child
        ///  subtrees happens immediately afterward (subject to
        ///  <paramref name="recurseMatching"/>).
        /// </remarks>
        public static List<TNode> GetChildrenOfType<TNode>(
            this Node node,
            bool recurse = false,
            bool recurseMatching = false
        )
            where TNode : Node
        {
            List<TNode> wantedChildren = [];

            foreach (var child in node.GetChildren())
            {
                if (child is TNode wantedChild)
                {
                    wantedChildren.Add(wantedChild);

                    // Skip recursion if type matches.
                    if (!recurseMatching)
                    {
                        continue;
                    }
                }

                if (recurse && child.GetChildCount() > 0)
                {
                    wantedChildren.AddRange(child.GetChildrenOfType<TNode>(true, recurseMatching));
                }
            }

            return wantedChildren;
        }
    }
}
