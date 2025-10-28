namespace Logic.Domain.NintendoManagement.Contract.DataClasses.Archive;

public class Sha1Section
{
    public byte[] arm9HmacHash;
    public byte[] arm7HmacHash;
    public byte[] digestMasterHmacHash;
    public byte[] iconHmacHash;
    public byte[] arm9iHmacHash;
    public byte[] arm7iHmacHash;
    public byte[] reserved1;
    public byte[] reserved2;
    public byte[] arm9HmacHashWithoutSecureArea;
    public byte[] reserved3;
    public byte[] dbgVariableStorage;   // zero-filled in rom
    public byte[] headerSectionRsa;
}