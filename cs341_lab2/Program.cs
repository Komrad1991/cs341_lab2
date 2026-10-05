using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        string first = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(first)) return;
        int m = int.Parse(first.Trim());

        var idByKey = new Dictionary<string, int>();
        var xs = new List<int>();
        var ys = new List<int>();
        var adj = new List<List<int>>();

        int GetId(int x, int y)
        {
            string key = x + "," + y;
            if (idByKey.TryGetValue(key, out int id))
                return id;
            id = xs.Count;
            idByKey[key] = id;
            xs.Add(x);
            ys.Add(y);
            adj.Add(new List<int>());
            return id;
        }

        for (int i = 0; i < m; i++)
        {
            string line;
            do { line = Console.ReadLine(); } while (line != null && string.IsNullOrWhiteSpace(line));
            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int x1 = int.Parse(parts[0]);
            int y1 = int.Parse(parts[1]);
            int x2 = int.Parse(parts[2]);
            int y2 = int.Parse(parts[3]);

            int a = GetId(x1, y1);
            int b = GetId(x2, y2);
            adj[a].Add(b);
            adj[b].Add(a);
        }

        int n = xs.Count;

        // true  -> BFS (фамилия на А-Н)
        // false -> DFS (фамилия на О-Я)
        bool useBfs = true;

        List<int> cycle = useBfs
            ? FindCycleBFS(adj, n)
            : FindCycleDFS(adj, n);

        cycle = NormalizeCycle(cycle, xs, ys);

        Console.WriteLine(cycle.Count);
        foreach (int v in cycle)
            Console.WriteLine(xs[v] + " " + ys[v]);
    }

    static List<int> FindCycleBFS(List<List<int>> adj, int n)
    {
        bool[] visited = new bool[n];
        int[] parent = new int[n];
        int[] depth = new int[n];
        for (int i = 0; i < n; i++) parent[i] = -1;

        for (int start = 0; start < n; start++)
        {
            if (visited[start]) continue;
            var q = new Queue<int>();
            visited[start] = true;
            parent[start] = -1;
            depth[start] = 0;
            q.Enqueue(start);

            while (q.Count > 0)
            {
                int u = q.Dequeue();
                foreach (int v in adj[u])
                {
                    if (!visited[v])
                    {
                        visited[v] = true;
                        parent[v] = u;
                        depth[v] = depth[u] + 1;
                        q.Enqueue(v);
                    }
                    else if (v != parent[u])
                    {
                        return BuildCycle(u, v, parent, depth);
                    }
                }
            }
        }
        return new List<int>();
    }

    static List<int> FindCycleDFS(List<List<int>> adj, int n)
    {
        bool[] visited = new bool[n];
        int[] parent = new int[n];
        int[] depth = new int[n];
        for (int i = 0; i < n; i++) parent[i] = -1;

        for (int start = 0; start < n; start++)
        {
            if (visited[start]) continue;
            var stack = new Stack<int>();
            visited[start] = true;
            parent[start] = -1;
            depth[start] = 0;
            stack.Push(start);

            while (stack.Count > 0)
            {
                int u = stack.Pop();
                foreach (int v in adj[u])
                {
                    if (!visited[v])
                    {
                        visited[v] = true;
                        parent[v] = u;
                        depth[v] = depth[u] + 1;
                        stack.Push(v);
                    }
                    else if (v != parent[u])
                    {
                        return BuildCycle(u, v, parent, depth);
                    }
                }
            }
        }
        return new List<int>();
    }

    static List<int> BuildCycle(int u, int v, int[] parent, int[] depth)
    {
        int a = u, b = v;
        while (depth[a] > depth[b]) a = parent[a];
        while (depth[b] > depth[a]) b = parent[b];
        while (a != b)
        {
            a = parent[a];
            b = parent[b];
        }
        int lca = a;

        var left = new List<int>();
        for (int cur = u; cur != lca; cur = parent[cur]) left.Add(cur);
        left.Add(lca);

        var right = new List<int>();
        for (int cur = v; cur != lca; cur = parent[cur]) right.Add(cur);
        right.Reverse();
        left.AddRange(right);
        return left;
    }

    static List<int> NormalizeCycle(List<int> cycle, List<int> xs, List<int> ys)
    {
        if (cycle.Count == 0) return cycle;

        // Ищем вершину с минимальными (x, y)
        int minIdx = 0;
        for (int i = 1; i < cycle.Count; i++)
        {
            int v = cycle[i];
            int curMin = cycle[minIdx];
            if (xs[v] < xs[curMin] || (xs[v] == xs[curMin] && ys[v] < ys[curMin]))
                minIdx = i;
        }

        // Циклический сдвиг, чтобы minIdx был первым
        var rotated = new List<int>();
        for (int i = 0; i < cycle.Count; i++)
            rotated.Add(cycle[(minIdx + i) % cycle.Count]);

        // Выбираем направление: следующий сосед должен быть меньше предыдущего
        if (rotated.Count > 1)
        {
            int first = rotated[0];
            int next = rotated[1];
            int prev = rotated[rotated.Count - 1];

            bool prevSmaller = xs[prev] < xs[next] || (xs[prev] == xs[next] && ys[prev] < ys[next]);
            if (prevSmaller)
            {
                // Разворачиваем подсписок, начиная с индекса 1
                var temp = rotated.GetRange(1, rotated.Count - 1);
                temp.Reverse();
                rotated = new List<int> { first };
                rotated.AddRange(temp);
            }
        }

        return rotated;
    }
}