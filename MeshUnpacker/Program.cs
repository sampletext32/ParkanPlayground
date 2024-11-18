// See https://aka.ms/new-console-template for more information

Console.WriteLine("Hello, World!");

using var fs = new FileStream("C:\\ParkanUnpacked\\11_fr_e_brige.msh\\0_fr_e_brige.bin", FileMode.Open);

byte[] buffer = new byte[38];

for (int i = 0; i < 6; i++)
{
    fs.ReadExactly(buffer);

    Console.WriteLine(string.Join(" ", buffer.Select(x => x.ToString("X2"))));
}