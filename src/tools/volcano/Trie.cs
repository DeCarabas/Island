namespace Volcano
{
    using System;
    using System.Collections.Generic;

    class Trie<T>
    {
        TrieNode root = new TrieNode();

        public void Add(string key, T obj)
        {
            TrieNode currentNode = this.root;
            int stringStart = 0;

            this.root.Items.Add(obj);
            if (key.Length == 0) { return; }

            while (true)
            {
                int childIndex = -1;
                for (int iChild = 0; iChild < currentNode.Children.Count; iChild++)
                {
                    if (currentNode.Children[iChild].Fragment[0] == key[stringStart])
                    {
                        childIndex = iChild;
                        break;
                    }
                }

                if (childIndex < 0)
                {
                    // Didn't find any maches; new node required as a child of this one.
                    //
                    currentNode.Children.Add(new TrieNode
                    {
                        Fragment = key.ToCharArray(stringStart, key.Length - stringStart),
                        Items = { obj },
                    });

                    return;
                }
                else
                {
                    TrieNode child = currentNode.Children[childIndex];
                    child.Items.Add(obj);

                    for (int i = 1; i < child.Fragment.Length; i++)
                    {
                        if (stringStart + i == key.Length)
                        {
                            // The key is a prefix of the fragment of this child. Split the child
                            // in twain, and add the object to the node with the fragment of the 
                            // prefix.
                            //
                            TrieNode newChild = SplitNode(child, i);
                            currentNode.Children[childIndex] = newChild;

                            newChild.Items.Add(obj);
                            return;
                        }
                        else if (child.Fragment[i] != key[stringStart + i])
                        {
                            // The provided key differs from the existing child at this point; we 
                            // must first split the child at this point, and then add a new node
                            // off the left part.
                            //
                            TrieNode newRoot = SplitNode(child, i);
                            currentNode.Children[childIndex] = newRoot;

                            newRoot.Children.Add(new TrieNode
                            {
                                Fragment = key.ToCharArray(stringStart + i, key.Length - (stringStart + i)),
                                Items = { obj },
                            });
                            return;
                        }
                    }

                    stringStart += child.Fragment.Length;
                    if (stringStart == key.Length)
                    {
                        // Exact match on this node, add and move on.
                        child.Items.Add(obj);
                        return;
                    }

                    // This child's fragment is a prefix of the key; this child should become the current node
                    // and we should continue on.
                    //
                    currentNode = child;
                }
            }
        }

        public ICollection<T> Lookup(string key)
        {
            int keyIndex = 0;
            TrieNode currentNode = this.root;

            if (key.Length == 0) { return this.root.Items; }

            while (true)
            {
                TrieNode candidate = null;
                foreach (TrieNode child in currentNode.Children)
                {
                    if (child.Fragment[0] == key[keyIndex])
                    {
                        candidate = child;
                        break;
                    }
                }
                if (candidate == null) { return new T[0]; }
                for (int i = 1; i < candidate.Fragment.Length; i++)
                {
                    // Key is prefix of this fragment; found what we're looking for!
                    if (keyIndex + i == key.Length) { return candidate.Items; }
                    if (candidate.Fragment[i] != key[keyIndex + i])
                    {
                        // Key diverges from this node; found nothing.
                        return new T[0];
                    }
                }

                // Key is exact match of this node
                //
                keyIndex += candidate.Fragment.Length;
                if (keyIndex == key.Length) { return candidate.Items; }

                // Node fragment is prefix; keep looking
                //
                currentNode = candidate;
            }
        }

        TrieNode SplitNode(TrieNode node, int offset)
        {
            // Reached the end of the provided string; we must split the child into 
            // two parts, and put the new object at the end of this one.
            //
            char[] parentFragment = new char[offset];
            Array.Copy(node.Fragment, 0, parentFragment, 0, offset);

            TrieNode newParent = new TrieNode
            {
                Fragment = parentFragment,
                Children = { node },
            };
            newParent.Items.UnionWith(node.Items);

            // The new child has a subset of its original fragment.
            //
            char[] childFragment = new char[node.Fragment.Length - offset];
            Array.Copy(node.Fragment, offset, childFragment, 0, childFragment.Length);
            node.Fragment = childFragment;

            return newParent;
        }

        class TrieNode
        {
            List<TrieNode> children;
            HashSet<T> items = new HashSet<T>();

            public List<TrieNode> Children
            {
                get
                {
                    if (this.children == null) { this.children = new List<TrieNode>(); }
                    return this.children;
                }
            }
            public char[] Fragment { get; set; }
            public HashSet<T> Items { get { return this.items; } }
        }
    }
}