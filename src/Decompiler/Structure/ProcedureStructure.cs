/* 
 * Copyright (C) 1999-2009 John K�ll�n.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2, or (at your option)
 * any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; see the file COPYING.  If not, write to
 * the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 */

using Decompiler.Core;
using Decompiler.Core.Code;
using Decompiler.Core.Lib;
using Decompiler.Structure;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Decompiler.Structure
{
    public abstract class GraphNode<T>
    {
        private List<T> inEdges;				// pointers to the nodes on an in edge to this node
        private List<T> outEdges;				// pointers to the nodes on an out edge from this node

        public GraphNode()
        {
            inEdges = new List<T>();
            outEdges = new List<T>();
        }

        public List<T> InEdges { get { return inEdges; } }
        public List<T> OutEdges { get { return outEdges; } }
    }

    /// <summary>
    /// Collects structure-related data associated with a Procedure.
    /// </summary>
    public class ProcedureStructure
    {
        private Procedure proc;
        private string name;
        private List<StructureNode> nodes;
        private StructureNode exitNode;
        private StructureNode entryNode;
        private List<StructureNode> ordering;
        private List<StructureNode> revOrdering;
        private List<DerivedGraph> derivedGraphs;	// the derived graphs for this procedure

        public ProcedureStructure(Procedure proc, List<StructureNode> nodes)
        {
            this.proc = proc;
            this.name = proc.Name;
            this.nodes = nodes;
            this.derivedGraphs = new List<DerivedGraph>();
            this.ordering = new List<StructureNode>();
            this.revOrdering = new List<StructureNode>();
        }


        public List<StructureNode> Nodes
        {
            get { return nodes; }
        }

        public StructureNode EntryNode
        {
            get { return entryNode; }
            set { entryNode = value; }
        }

        public StructureNode ExitNode
        {
            get { return exitNode; }
            set { exitNode = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public List<Decompiler.Structure.DerivedGraph> DerivedGraphs
        {
            get { return derivedGraphs; }
        }

        public List<StructureNode> Ordering
        {
            get { return ordering; }
        }

        // within this procedure such that the nodes lower in
        // graph are earlier in the array

        public List<StructureNode> ReverseOrdering
        {
            get { return revOrdering; }
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            StringWriter sw = new StringWriter();
            Write(sw);
            Debug.WriteLine(sw.ToString());
        }

        public void Write(System.IO.TextWriter writer)
        {
            WriteNode(this.entryNode, new HashSet<StructureNode>(), writer);
        }

        [Conditional("DEBUG")]
        private void WriteNode(StructureNode node, HashSet<StructureNode> visited, System.IO.TextWriter writer)
        {
            if (visited.Contains(node))
                return;
            visited.Add(node);
            writer.WriteLine("Node {0}: Block: {1}",
                node.Ident(),
                node.Block != null ? node.Block.Name : "<none>");

            writer.WriteLine("    Order: {0}, RevOrder {1}", node.Order, node.RevOrder);
            writer.WriteLine("    Structure type: {0}", node.GetStructType());
            if (node.LoopHead != null)
                writer.WriteLine("    Loop header:" + node.LoopHead.Block.Name);
            if (node.LatchNode != null)
                writer.WriteLine("    Latch: {0}", node.LatchNode.Block.Name);
            if (node.CondFollow != null)
                writer.WriteLine("    Cond follow: {0}", node.CondFollow.Block.Name);
            writer.WriteLine("    Unstructured type: {0}", node.UnstructType);

            writer.Write("    Succ: ");
            string sep = "";
            foreach (StructureNode s in node.OutEdges)
            {
                writer.Write(sep);
                writer.Write(s.Block.Name);
                sep = ",";
            }
            writer.WriteLine();

            foreach (StructureNode s in node.OutEdges)
            {
                WriteNode(s, visited, writer);
            }
        }

    }
}