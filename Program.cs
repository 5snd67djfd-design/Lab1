using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
struct MainData { public string protein; public string organism; public string aminoAcid; }
class Program
{
    // проверка на наличие
    static StreamWriter InitializeProcessing(string sequenceFile, string commandFile, string outputFile, out List<MainData> geneticDataList)
    {
        geneticDataList = LoadMainData(sequenceFile);

        if (geneticDataList.Count == 0)
        {
            Console.WriteLine($"Файл {sequenceFile} не найден или пуст.");
            return null;
        }

        if (!File.Exists(commandFile))
        {
            Console.WriteLine($"Файл команд {commandFile} не найден.");
            return null;
        }

        return new StreamWriter(outputFile, false, Encoding.UTF8);
    }
    // загрузка данных
    static List<MainData> LoadMainData(string filePath)
    {
        List<MainData> list = new List<MainData>();
        if (!File.Exists(filePath)) return list;

        string[] lines = File.ReadAllLines(filePath);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split('\t');
            if (parts.Length >= 3)
            {
                MainData data = new MainData
                {
                    protein = parts[0].Trim(),
                    organism = parts[1].Trim(),
                    aminoAcid = Decoding(parts[2].Trim())
                };
                list.Add(data);
            }
        }
        return list;
    }

    // ищет заданный фрагмент кислоты
    static void ProcessSearch(int opNum, string rawPattern, List<MainData> dataList, StreamWriter writer)
    {
        string pattern = Decoding(rawPattern);

        writer.WriteLine($"{opNum:D3} search\t{pattern}");
        writer.WriteLine("organism\tprotein");

        bool found = false;
        foreach (var data in dataList)
        {
            if (data.aminoAcid.Contains(pattern))
            {
                writer.WriteLine($"{data.organism}\t{data.protein}");
                found = true;
            }
        }

        if (!found)
        {
            writer.WriteLine("NOT FOUND");
        }

        writer.WriteLine("--------------------------------------------------");
    }
    // находит раздичия 
    static void ProcessDiff(int opNum, string protein1Name, string protein2Name, List<MainData> dataList, StreamWriter writer)
    {
        writer.WriteLine($"{opNum:D3} diff\t{protein1Name}\t{protein2Name}");
        writer.WriteLine("amino-acids difference:");

        int p1Index = dataList.FindIndex(d => d.protein == protein1Name);
        int p2Index = dataList.FindIndex(d => d.protein == protein2Name);

        bool p1Missing = p1Index == -1;
        bool p2Missing = p2Index == -1;

        if (p1Missing || p2Missing)
        {
            List<string> missing = new List<string>();
            if (p1Missing) missing.Add(protein1Name);
            if (p2Missing) missing.Add(protein2Name);

            writer.WriteLine($"MISSING: {string.Join(", ", missing)}");
        }
        else
        {
            string seq1 = dataList[p1Index].aminoAcid;
            string seq2 = dataList[p2Index].aminoAcid;

            int minLen = Math.Min(seq1.Length, seq2.Length);
            int diffCount = 0;

            for (int i = 0; i < minLen; i++)
            {
                if (seq1[i] != seq2[i]) diffCount++;
            }

            diffCount += Math.Abs(seq1.Length - seq2.Length);

            writer.WriteLine(diffCount);
        }

        writer.WriteLine("--------------------------------------------------");
    }
    // самая частая кислота
    static void ProcessMode(int opNum, string proteinName, List<MainData> dataList, StreamWriter writer)
    {
        writer.WriteLine($"{opNum:D3} mode\t{proteinName}");
        writer.WriteLine("amino-acid\toccurs:");

        int pIndex = dataList.FindIndex(d => d.protein == proteinName);

        if (pIndex == -1)
        {
            writer.WriteLine($"MISSING: {proteinName}");
        }
        else
        {
            string seq = dataList[pIndex].aminoAcid;
            Dictionary<char, int> counts = new Dictionary<char, int>();

            foreach (char c in seq)
            {
                if (counts.ContainsKey(c))
                    counts[c]++;
                else
                    counts[c] = 1;
            }

            var top = counts
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .First();

            writer.WriteLine($"{top.Key}\t{top.Value}");
        }

        writer.WriteLine("--------------------------------------------------");
    }
}