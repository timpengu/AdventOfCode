using MoreLinq;
using System.Diagnostics;

internal static class Program
{
    static int VerboseLevel = 2;

    const int FreeId = -1;

    public static void Main(string[] args)
    {
        List<(int FileLen, int FreeLen)> input = new(
            File.ReadLines("inputSample.txt")
                .Single()
                .Select(ToDigit)
                .Batch(2, d => (d.First(), d.Skip(1).FirstOrDefault()))
        );

        Console.WriteLine("Compact blocks...");
        Console.WriteLine("Checksum: {0}\n", input.CompactBlocks().Select(GetChecksum).Sum());

        Console.WriteLine("Compact files...");
        Console.WriteLine("Checksum: {0}\n", input.CompactFiles().Select(GetChecksum).Sum());
    }

    static IEnumerable<int> CompactBlocks(this IEnumerable<(int FileLen, int FreeLen)> input)
    {
        List<int> blocks = new(input.UnpackBlocks());

        int idxFree = blocks.FindIndex(IsFree);
        int idxFile = blocks.FindLastIndex(IsFile);

        while (idxFree > 0 && idxFile > 0 && idxFree < idxFile)
        {
            if (VerboseLevel >= 2)
            {
                WriteBlocksToConsole(blocks);
            }

            Debug.Assert(blocks[idxFree].IsFree());
            Debug.Assert(blocks[idxFile].IsFile());

            blocks[idxFree] = blocks[idxFile]; // move file block to free block
            blocks[idxFile] = FreeId; // free original file block

            idxFree = blocks.FindIndex(idxFree, IsFree);
            idxFile = blocks.FindLastIndex(idxFile, IsFile);
        }

        if (VerboseLevel >= 1)
        {
            WriteBlocksToConsole(blocks);
        }

        return blocks;
    }

    static IEnumerable<int> CompactFiles(this IEnumerable<(int FileLen, int FreeLen)> input)
    {
        (List<Chunk> fileList, List<Chunk> freeList) = input
            .UnpackChunks() // in block order
            .Partition(IsFile, (file, free) => (file.ToList(), free.ToList()));

        int totalLen = fileList.Sum(f => f.Len) + freeList.Sum(f => f.Len);

        for (int fileIndex = fileList.Count - 1; fileIndex >= 0; --fileIndex)
        {
            if (VerboseLevel >= 2)
            {
                WriteBlocksToConsole(fileList.ToBlocks(totalLen));
            }

            Chunk file = fileList[fileIndex];

            int freeIndex = Enumerable.Range(0, freeList.Count)
                .TakeWhile(i => freeList[i].Block < file.Block) // stop search when free.Block >= file.Block
                .FirstOrDefault(i => freeList[i].Len >= file.Len , -1);

            if (freeIndex >= 0)
            {
                Chunk free = freeList[freeIndex];
                fileList[fileIndex] = new Chunk(file.Id, free.Block, file.Len); // move file to free block
                freeList[freeIndex] = new Chunk(FreeId, free.Block + file.Len, free.Len - file.Len); // reduce free chunk
            }
        }

        List<int> blocks = fileList.ToBlocks(totalLen).ToList();

        if (VerboseLevel >= 1)
        {
            WriteBlocksToConsole(blocks);
        }

        return blocks;
    }

    static IEnumerable<int> ToBlocks(this IEnumerable<Chunk> files, int minBlocks = 0)
    {
        int block = 0;
        
        foreach (Chunk file in files.Where(IsFile).OrderBy(f => f.Block))
        {
            for (; block < file.Block; ++block) yield return FreeId;
            
            for(; block < file.Block + file.Len; ++block) yield return file.Id;
        }

        for (; block < minBlocks; ++block) yield return FreeId;
    }

    static IEnumerable<int> UnpackBlocks(this IEnumerable<(int FileLen, int FreeLen)> input) =>
        input.SelectMany((item, id) =>
            Enumerable.Concat(
                Enumerable.Repeat(id, item.FileLen),
                Enumerable.Repeat(FreeId, item.FreeLen)));

    static IEnumerable<Chunk> UnpackChunks(this IEnumerable<(int FileLen, int FreeLen)> input)
    {
        int blockIndex = 0;
        int fileId = 0;
        foreach ((int fileLen, int freeLen) in input)
        {
            yield return new Chunk(fileId++, blockIndex, fileLen);
            blockIndex += fileLen;

            yield return new Chunk(FreeId, blockIndex, freeLen);
            blockIndex += freeLen;
        }
    }

    static void WriteBlocksToConsole(IEnumerable<int> blocks)
    {
        ConsoleColor[] colours = { ConsoleColor.Red, ConsoleColor.Yellow, ConsoleColor.Green, ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Magenta };
        foreach(int id in blocks)
        {
            Console.ForegroundColor = id.IsFree() ? ConsoleColor.White : colours[id % colours.Length];
            Console.Write(id.IsFree() ? ". " : $"{id} ");
        }
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    record struct Chunk(int Id, int Block, int Len);

    static long GetChecksum(int id, int blockIdx) => id.IsFile() ? blockIdx * (long)id : 0;

    static bool IsFree(this Chunk f) => f.Id.IsFree();
    static bool IsFile(this Chunk f) => f.Id.IsFile();
    static bool IsFree(this int id) => id == FreeId;
    static bool IsFile(this int id) => id != FreeId;

    static int ToDigit(this char c) =>
        c >= '0' && c <= '9'
            ? c - '0'
            : throw new ArgumentException($"Invalid digit: {c}", nameof(c));
}