namespace WibboEmulator.Utilities;
using System.Collections;
using System.Net;
using System.Net.Sockets;

public class IPRange
{
    public static bool IsInSubnet(IPAddress address, string subnetMask)
    {
        var slashIdx = subnetMask.IndexOf('/');
        if (slashIdx == -1)
        { // We only handle netmasks in format "IP/PrefixLength".
            return false;
        }

        // First parse the address of the netmask before the prefix length.
        var maskAddress = IPAddress.Parse(subnetMask.AsSpan(0, slashIdx));

        if (maskAddress.AddressFamily != address.AddressFamily)
        { // We got something like an IPV4-Address for an IPv6-Mask. This is not valid.
            return false;
        }

        // Now find out how long the prefix is.
        var maskLength = int.Parse(subnetMask[(slashIdx + 1)..]);

        if (maskLength == 0)
        {
            return false;
        }

        if (maskLength < 0)
        {
            return false;
        }

        if (maskAddress.AddressFamily == AddressFamily.InterNetwork)
        {
            // Convert the mask address to an unsigned integer.
            var maskAddressBits = BitConverter.ToUInt32(maskAddress.GetAddressBytes().Reverse().ToArray(), 0);

            // And convert the IpAddress to an unsigned integer.
            var ipAddressBits = BitConverter.ToUInt32(address.GetAddressBytes().Reverse().ToArray(), 0);

            // Get the mask/network address as unsigned integer.
            var mask = uint.MaxValue << (32 - maskLength);

            // https://stackoverflow.com/a/1499284/3085985
            // Bitwise AND mask and MaskAddress, this should be the same as mask and IpAddress
            // as the end of the mask is 0000 which leads to both addresses to end with 0000
            // and to start with the prefix.
            return (maskAddressBits & mask) == (ipAddressBits & mask);
        }

        if (maskAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            // Convert the mask address to a BitArray. Reverse the BitArray to compare the bits of each byte in the right order.
            var maskAddressBits = new BitArray(maskAddress.GetAddressBytes().Reverse().ToArray());

            // And convert the IpAddress to a BitArray. Reverse the BitArray to compare the bits of each byte in the right order.
            var ipAddressBits = new BitArray(address.GetAddressBytes().Reverse().ToArray());
            var ipAddressLength = ipAddressBits.Length;

            if (maskAddressBits.Length != ipAddressBits.Length)
            {
                throw new ArgumentException("Length of IP Address and Subnet Mask do not match.");
            }

            // Compare the prefix bits.
            for (var i = ipAddressLength - 1; i >= ipAddressLength - maskLength; i--)
            {
                if (ipAddressBits[i] != maskAddressBits[i])
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}
