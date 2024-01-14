namespace WibboEmulator.WebSocketSharp.Net;

#region License
/*
 * EndPointManager.cs
 *
 * This code is derived from EndPointManager.cs (System.Net) of Mono
 * (http://www.mono-project.com).
 *
 * The MIT License
 *
 * Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
 * Copyright (c) 2012-2020 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

#region Authors
/*
 * Authors:
 * - Gonzalo Paniagua Javier <gonzalo@ximian.com>
 */
#endregion

#region Contributors
/*
 * Contributors:
 * - Liryna <liryna.stark@gmail.com>
 */
#endregion

using System.Collections;
using System.Collections.Generic;
using System.Net;
using WibboEmulator.WebSocketSharp;

internal sealed class EndPointManager
{
    #region Private Fields

    private static readonly Dictionary<IPEndPoint, EndPointListener> Endpoints;

    #endregion

    #region Static Constructor

    static EndPointManager() => Endpoints = new Dictionary<IPEndPoint, EndPointListener>();

    #endregion

    #region Private Constructors

    private EndPointManager()
    {
    }

    #endregion

    #region Private Methods

    private static void AddPrefixEnd(string uriPrefix, HttpListener listener)
    {
        var pref = new HttpListenerPrefix(uriPrefix, listener);

        var addr = ConvertToIPAddress(pref.Host);

        if (addr == null)
        {
            var msg = "The URI prefix includes an invalid host.";

            throw new HttpListenerException(87, msg);
        }

        if (!addr.IsLocal())
        {
            var msg = "The URI prefix includes an invalid host.";

            throw new HttpListenerException(87, msg);
        }


        if (!int.TryParse(pref.Port, out var port))
        {
            var msg = "The URI prefix includes an invalid port.";

            throw new HttpListenerException(87, msg);
        }

        if (!port.IsPortNumber())
        {
            var msg = "The URI prefix includes an invalid port.";

            throw new HttpListenerException(87, msg);
        }

        var path = pref.Path;

        if (path.Contains('%'))
        {
            var msg = "The URI prefix includes an invalid path.";

            throw new HttpListenerException(87, msg);
        }

        if (path.Contains("//"))
        {
            var msg = "The URI prefix includes an invalid path.";

            throw new HttpListenerException(87, msg);
        }

        var endpoint = new IPEndPoint(addr, port);


        if (Endpoints.TryGetValue(endpoint, out var lsnr))
        {
            if (lsnr.IsSecure ^ pref.IsSecure)
            {
                var msg = "The URI prefix includes an invalid scheme.";

                throw new HttpListenerException(87, msg);
            }
        }
        else
        {
            lsnr = new EndPointListener(
                     endpoint,
                     pref.IsSecure,
                     listener.CertificateFolderPath,
                     listener.SslConfiguration,
                     listener.ReuseAddress
                   );

            Endpoints.Add(endpoint, lsnr);
        }

        lsnr.AddPrefix(pref);
    }

    private static IPAddress ConvertToIPAddress(string hostname)
    {
        if (hostname == "*")
        {
            return IPAddress.Any;
        }

        if (hostname == "+")
        {
            return IPAddress.Any;
        }

        return hostname.ToIPAddress();
    }

    private static void RemovePrefixEnd(string uriPrefix, HttpListener listener)
    {
        var pref = new HttpListenerPrefix(uriPrefix, listener);

        var addr = ConvertToIPAddress(pref.Host);

        if (addr == null)
        {
            return;
        }

        if (!addr.IsLocal())
        {
            return;
        }


        if (!int.TryParse(pref.Port, out var port))
        {
            return;
        }

        if (!port.IsPortNumber())
        {
            return;
        }

        var path = pref.Path;

        if (path.Contains('%'))
        {
            return;
        }

        if (path.Contains("//"))
        {
            return;
        }

        var endpoint = new IPEndPoint(addr, port);


        if (!Endpoints.TryGetValue(endpoint, out var lsnr))
        {
            return;
        }

        if (lsnr.IsSecure ^ pref.IsSecure)
        {
            return;
        }

        lsnr.RemovePrefix(pref);
    }

    #endregion

    #region Internal Methods

    internal static bool RemoveEndPoint(IPEndPoint endpoint)
    {
        lock (((ICollection)Endpoints).SyncRoot)
        {
            return Endpoints.Remove(endpoint);
        }
    }

    #endregion

    #region Public Methods

    public static void AddListener(HttpListener listener)
    {
        var added = new List<string>();

        lock (((ICollection)Endpoints).SyncRoot)
        {
            try
            {
                foreach (var pref in listener.Prefixes)
                {
                    AddPrefixEnd(pref, listener);
                    added.Add(pref);
                }
            }
            catch
            {
                foreach (var pref in added)
                {
                    RemovePrefixEnd(pref, listener);
                }

                throw;
            }
        }
    }

    public static void AddPrefix(string uriPrefix, HttpListener listener)
    {
        lock (((ICollection)Endpoints).SyncRoot)
        {
            AddPrefixEnd(uriPrefix, listener);
        }
    }

    public static void RemoveListener(HttpListener listener)
    {
        lock (((ICollection)Endpoints).SyncRoot)
        {
            foreach (var pref in listener.Prefixes)
            {
                RemovePrefixEnd(pref, listener);
            }
        }
    }

    public static void RemovePrefix(string uriPrefix, HttpListener listener)
    {
        lock (((ICollection)Endpoints).SyncRoot)
        {
            RemovePrefixEnd(uriPrefix, listener);
        }
    }

    #endregion
}
