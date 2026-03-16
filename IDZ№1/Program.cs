using System;

while (true)
{
    Console.Write("Введите первую строку: ");
    string firstInput = Console.ReadLine();
    
    if (firstInput == "exit") break;

    Console.WriteLine();
    Console.Write("Введите вторую строку: ");
    string secondInput = Console.ReadLine();

    DisplayLevenshtein(firstInput, secondInput);
}

static int CalculateDistance(string source, string target)
{
    if (source == null || target == null) return -1;

    int sourceLen = source.Length;
    int targetLen = target.Length;

    if (sourceLen == 0) return targetLen;
    if (targetLen == 0) return sourceLen;

    string s1 = source.ToUpper();
    string s2 = target.ToUpper();

    int[,] distMatrix = new int[sourceLen + 1, targetLen + 1];

    for (int i = 0; i <= sourceLen; i++) distMatrix[i, 0] = i;
    for (int j = 0; j <= targetLen; j++) distMatrix[0, j] = j;

    for (int i = 1; i <= sourceLen; i++)
    {
        for (int j = 1; j <= targetLen; j++)
        {
            int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

            int insertion = distMatrix[i, j - 1] + 1;
            int deletion = distMatrix[i - 1, j] + 1;
            int substitution = distMatrix[i - 1, j - 1] + cost;

            int minStep = Math.Min(Math.Min(insertion, deletion), substitution);
            distMatrix[i, j] = minStep;

            if (i > 1 && j > 1 && s1[i - 1] == s2[j - 2] && s1[i - 2] == s2[j - 1])
            {
                distMatrix[i, j] = Math.Min(distMatrix[i, j], distMatrix[i - 2, j - 2] + cost);
            }
        }
    }
    return distMatrix[sourceLen, targetLen];
}

static void DisplayLevenshtein(string a, string b)
{
    int result = CalculateDistance(a, b);
    Console.WriteLine($"'{a}, {b}' -> {result}");
}
