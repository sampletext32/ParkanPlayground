using Silk.NET.OpenGL;
using TexmLib;

namespace NResUI.Models;

public class TexmExplorerViewModel
{
    public bool HasFile { get; set; }
    public string? Error { get; set; }

    public TexmFile? TexmFile { get; set; }

    public string? Path { get; set; }
    public List<OpenGlTexture> GlTextures { get; set; } = [];

    private bool _glTexturesDirty = false;
    public bool IsWhiteBgEnabled;

    public bool IsBlackBgEnabled;
    public bool DoubleSize;

    public void SetParseResult(TexmParseResult result, string path)
    {
        Error = result.Error;

        if (result.TexmFile != null)
        {
            HasFile = true;
        }

        TexmFile = result.TexmFile;
        Path = path;
        _glTexturesDirty = true;
    }

    /// <summary>
    /// Сгенерировать OpenGL текстуры из всех мипмапов Texm файла
    /// </summary>
    public void GenerateGlTextures(GL gl)
    {
        if (_glTexturesDirty && TexmFile is not null)
        {
            foreach (var glTexture in GlTextures)
            {
                glTexture.Dispose();
            }

            GlTextures.Clear();

            for (var i = 0; i < TexmFile!.Header.MipmapCount; i++)
            {
                var bytes = TexmFile.GetRgba32BytesFromMipmap(i, out var width, out var height);

                var glTexture = new OpenGlTexture(
                    gl,
                    width,
                    height,
                    bytes
                );

                GlTextures.Add(glTexture);
            }

            _glTexturesDirty = false;
        }
    }
}