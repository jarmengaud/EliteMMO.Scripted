﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace EliteMMO.Scripted.Embedded
{
    public class SigScan
    {
        /// <summary>
        ///     m_vDumpedRegion
        ///     The memory dumped from the external process.
        /// </summary>
        private byte[] m_vDumpedRegion;

        /// <summary>
        ///     ReadProcessMemory
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        #region "sigScan Class Construction"

        /// <summary>
        ///     SigScan
        ///     Main class constructor that uses no params.
        ///     Simply initializes the class properties and
        ///     expects the user to set them later.
        /// </summary>
        public SigScan()
        {
            Process = null;
            Address = IntPtr.Zero;
            Size = 0;
            m_vDumpedRegion = null;
        }

        /// <summary>
        ///     SigScan
        ///     Overloaded class constructor that sets the class
        ///     properties during construction.
        /// </summary>
        /// <param name="proc">The process to dump the memory from.</param>
        /// <param name="addr">The started address to begin the dump.</param>
        /// <param name="size">The size of the dump.</param>
        public SigScan(Process proc, IntPtr addr, int size)
        {
            Process = proc;
            Address = addr;
            Size = size;
        }

        #endregion

        #region "sigScan Class Private Methods"

        /// <summary>
        ///     DumpMemory
        ///     Internal memory dump function that uses the set class
        ///     properties to dump a memory region.
        /// </summary>
        /// <returns>Boolean based on RPM results and valid properties.</returns>
        private bool DumpMemory()
        {
            try
            {
                // Checks to ensure we have valid data. 
                if (Process?.HasExited ?? true)
                    return false;
                if (Address == IntPtr.Zero)
                    return false;
                if (Size == 0)
                    return false;

                // Create the region space to dump into.
                m_vDumpedRegion = new byte[Size];

                int nBytesRead;

                // Dump the memory. 
                var ret = ReadProcessMemory(
                    Process.Handle, Address, m_vDumpedRegion, Size, out nBytesRead
                );

                // Validation checks. 
                return ret && nBytesRead == Size;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     MaskCheck
        ///     Compares the current pattern byte to the current memory dump
        ///     byte to check for a match. Uses wildcards to skip bytes that
        ///     are deemed unneeded in the compares.
        /// </summary>
        /// <param name="nOffset">Offset in the dump to start at.</param>
        /// <param name="btPattern">Pattern to scan for.</param>
        /// <param name="strMask">Mask to compare against.</param>
        /// <returns>Boolean depending on if the pattern was found.</returns>
        private bool MaskCheck(int nOffset, IEnumerable<byte> btPattern, string strMask)
        {
            return !btPattern
                .Where((t, x) => strMask[x] != '?' && strMask[x] == 'x' && t != m_vDumpedRegion[nOffset + x]).Any();
        }

        #endregion

        #region "sigScan Class Public Methods"

        /// <summary>
        ///     FindPattern
        ///     Attempts to locate the given pattern inside the dumped memory region
        ///     compared against the given mask. If the pattern is found, the offset
        ///     is added to the located address and returned to the user.
        /// </summary>
        /// <param name="btPattern">Byte pattern to look for in the dumped region.</param>
        /// <param name="strMask">The mask string to compare against.</param>
        /// <param name="nOffset">The offset added to the result address.</param>
        /// <returns>IntPtr - zero if not found, address if found.</returns>
        public IntPtr FindPattern(byte[] btPattern, string strMask, int nOffset)
        {
            try
            {
                // Dump the memory region if we have not dumped it yet. 
                if ((m_vDumpedRegion == null || m_vDumpedRegion.Length == 0) && !DumpMemory()) return IntPtr.Zero;

                // Ensure the mask and pattern lengths match. 
                if (strMask.Length != btPattern.Length)
                    return IntPtr.Zero;

                // Loop the region and look for the pattern. 
                for (var x = 0; x < m_vDumpedRegion.Length; x++)
                    if (MaskCheck(x, btPattern, strMask))
                        // The pattern was found, return it. 
                        return new IntPtr((int) Address + x + nOffset);

                // Pattern was not found. 
                return IntPtr.Zero;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        ///     ResetRegion
        ///     Resets the memory dump array to nothing to allow
        ///     the class to redump the memory.
        /// </summary>
        public void ResetRegion()
        {
            m_vDumpedRegion = null;
        }

        #endregion

        #region "sigScan Class Properties"

        public Process Process { get; set; }

        public IntPtr Address { get; set; }

        public int Size { get; set; }

        #endregion
    }
}