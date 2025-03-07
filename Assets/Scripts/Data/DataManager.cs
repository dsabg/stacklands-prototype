using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization;
using Systems;
using System;

namespace Data
{
    public class DataManager
    {
        public class DirectoryNode : IEnumerable<DirectoryNode>
        {
            public string Path { get; private set; }
            public Dictionary<string, DirectoryNode> Children = new Dictionary<string, DirectoryNode>();


            public bool Contains(string child)
            {
                return Children.ContainsKey(child);
            }

            public DirectoryNode this[string path]
            {
                get => Children[path];
                set => Children[path] = value;
            }
            
            public IEnumerator<DirectoryNode> GetEnumerator()
            {
                return Children.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            
            public DirectoryNode(string path, params KeyValuePair<string, DirectoryNode>[] children)
            {
                Path = path;
            }
        }

        public static event Action FinishedLoad;

        public static void LoadModules()
        {//它属于类本身，而不是类的实例。换句话说，静态方法不依赖于类的实例化，可以通过类名直接调用。静态方法不能访问类的实例字段或实例方法，它只能访问该类中的静态字段和其他静态方法。

            //获取StreamingAssets目录下的所有子目录路径. Application.streamingAssetsPath 是 Unity 提供的一个属性，返回当前项目中 StreamingAssets 文件夹的绝对路径。
            string[] modules = Directory.GetDirectories(Application.streamingAssetsPath);

            foreach(string module in modules)
            {
                LoadSingle(module);
            }
            //?. 是一个空值安全操作符，表示如果 FinishedLoad 事件有订阅者（即不为 null），则调用 Invoke() 方法触发事件；否则，什么也不做。
            FinishedLoad?.Invoke();
        }

        private static void LoadSingle(string module)
        {
            DirectoryNode root = BuildDirectoryTree(module);

            uint cardCount = LoadCards(root);
            uint loadPackage=LoadPackage(root);

            Debug.Log($"Loaded module {module}: Cards: {cardCount}");
            Debug.Log($"Loaded module {module}: Packages: {loadPackage}");
        }

        private static DirectoryNode BuildDirectoryTree(string root)
        {
            Stack<DirectoryNode> unexplored = new Stack<DirectoryNode>();
            DirectoryNode rootNode = new DirectoryNode(root);
            unexplored.Push(rootNode);

            DirectoryNode current;
            while (unexplored.Count > 0)
            {
                current = unexplored.Pop();
                string[] children = Directory.GetDirectories(current.Path);
                foreach (string child in children)
                {
                    DirectoryNode childNode = new DirectoryNode(child);
                    current.Children.Add(child, childNode);
                    unexplored.Push(childNode);
                }
            }

            return rootNode;
        }

        private static uint LoadCards(DirectoryNode moduleRoot)
        {
            uint loaded = 0;
            string cardPath = Path.Combine(moduleRoot.Path, "cards");
            if (moduleRoot.Children.TryGetValue(cardPath, out DirectoryNode cardRoot))
            {
                Stack<DirectoryNode> unexplored = new Stack<DirectoryNode>();
                unexplored.Push(cardRoot);

                DirectoryNode current;
                while (unexplored.Count > 0)
                {
                    current = unexplored.Pop();
                    StreamingContext context = new StreamingContext(StreamingContextStates.File, new CardDataContext(cardPath));
                    foreach (string file in Directory.EnumerateFiles(current.Path, "*.json"))
                    {
                        string json = File.ReadAllText(file);
                        CardData[] data = JsonConvert.DeserializeObject<CardData[]>(json, new JsonSerializerSettings() { Context = context });
                        foreach (CardData card in data)
                        {
                            CardManager.LoadedCards.Add(card.Name, card);
                            loaded++;
                        }
                    }

                    foreach (DirectoryNode child in current)
                    {
                        unexplored.Push(child);
                    }
                }
            }
            return loaded;
        }

        private static  uint LoadPackage(DirectoryNode moduleRoot)
        {
            uint loaded = 0;
            string packagePath = Path.Combine(moduleRoot.Path, "packages");

            if (moduleRoot.Children.TryGetValue(packagePath, out DirectoryNode cardRoot))
            {
                Stack<DirectoryNode> unexplored = new Stack<DirectoryNode>();
                unexplored.Push(cardRoot);

                DirectoryNode current;
                while (unexplored.Count > 0)
                {
                    current = unexplored.Pop();
                    
                    foreach (string file in Directory.EnumerateFiles(current.Path, "*.json"))
                    {
                        string json = File.ReadAllText(file);
                        PackageData[] data = JsonConvert.DeserializeObject<PackageData[]>(json, new JsonSerializerSettings() );
                        foreach (PackageData package in data)
                        {
                            CardManager.LoadedPackages.Add(package.Name,package);
                            loaded++;
                        }
                    }

                    foreach (DirectoryNode child in current)
                    {
                        unexplored.Push(child);
                    }
                }
            }

            return loaded;
        }
    } 
}
