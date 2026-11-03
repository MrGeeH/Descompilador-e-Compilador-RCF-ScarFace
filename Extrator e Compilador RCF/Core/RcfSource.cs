namespace Extrator_e_Compilador_RCF.Core
{
    /// <summary>De onde vem o conteúdo de um arquivo ao compilar o RCF.</summary>
    public abstract class RcfSource
    {
        public abstract long Length { get; }
        public abstract void CopyTo(Stream output, byte[] buffer, CancellationToken ct);
        public abstract byte[] ReadAll();

        /// <summary>Primeiros bytes do conteúdo (para reconhecer o tipo sem ler o arquivo inteiro).</summary>
        public virtual byte[] ReadHead(int count)
        {
            var all = ReadAll();
            return all.Length <= count ? all : all[..count];
        }
    }

    /// <summary>Arquivo no disco.</summary>
    public sealed class FileSource(string path) : RcfSource
    {
        public string Path { get; } = path;
        public override long Length => new FileInfo(Path).Length;
        public override void CopyTo(Stream output, byte[] buffer, CancellationToken ct)
        {
            using var input = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16);
            StreamUtil.Copy(input, output, input.Length, buffer, ct);
        }
        public override byte[] ReadAll() => File.ReadAllBytes(Path);
        public override byte[] ReadHead(int count)
        {
            using var input = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var data = new byte[Math.Min(count, input.Length)];
            input.ReadExactly(data);
            return data;
        }
    }

    /// <summary>Trecho de um RCF existente (arquivo não modificado).</summary>
    public sealed class ArchiveSource(string archivePath, long offset, long size) : RcfSource
    {
        public override long Length => size;
        public override void CopyTo(Stream output, byte[] buffer, CancellationToken ct)
        {
            using var input = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16);
            input.Position = offset;
            StreamUtil.Copy(input, output, size, buffer, ct);
        }
        public override byte[] ReadAll()
        {
            using var input = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var data = new byte[size];
            input.Position = offset;
            input.ReadExactly(data);
            return data;
        }
        public override byte[] ReadHead(int count)
        {
            using var input = new FileStream(archivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var data = new byte[Math.Min(count, size)];
            input.Position = offset;
            input.ReadExactly(data);
            return data;
        }
    }

    /// <summary>Conteúdo em memória (ex.: P3D com imagem trocada).</summary>
    public sealed class BytesSource(byte[] data) : RcfSource
    {
        public override long Length => data.Length;
        public override void CopyTo(Stream output, byte[] buffer, CancellationToken ct) => output.Write(data);
        public override byte[] ReadAll() => data;
    }
}
