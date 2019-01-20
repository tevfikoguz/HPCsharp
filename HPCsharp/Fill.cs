﻿// TODO: Use some suggestions from https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c and also parallelize if bandwidth is not used up by
//       a single core. Hope this works for ushort too.
// TODO: Can we allocate the count array on the stack to make sorting small arrays faster?
// TODO: Benchmark Fill() for different data types from byte (8-bits) to ulong (64-bits) to see if all of memory bandwidth can be used up. If not then go parallel.
// TODO: If Fill() benchmarks well for larger data types, then reading the array 64-bits at a time will most likely pay off as well. Yes it does! Great direction to go in.
// TODO: Consider using SIMD instructions to read and write even more bits per iteration - e.g. 256-bits is 32 bytes and 512-bits is an entire cache line.
//       https://stackoverflow.com/questions/31999479/using-simd-operation-from-c-sharp-in-net-framework-4-6-is-slower
// TODO: Make sure to mention that .NET core has a Fill method implemented already. Modify my version to have the same interface, and provide a parallel version that's even faster.
// TODO: Benchmark my Fill version on a quad-memory channel system to see how much bandwidth it provides - the fill rate! ;-) kinda like graphics.
// TODO: Implement serial Fill of byte, ushort and uint using ulong to accelerate it.
// TODO: Figure out how to make accelerated Fill generic, such as done for .NET core 2.X
// TODO: Change SSE Fill to look at alignment of the buffer first and do scalar up to 32-byte alignment and then do SSE - otherwise performance is abysmal.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp
{
    static public partial class Algorithm
    {
        public static void Fill<T>(this T[] arrayToFill, T value)
        {
            for (int i = 0; i < arrayToFill.Length; i++)
                arrayToFill[i] = value;
        }

        public static void Fill<T>(this T[] arrayToFill, T value, int startIndex, int length)
        {
            int index    = startIndex;
            int endIndex = startIndex + length;
            for (int i = startIndex; i < endIndex; i++)
                arrayToFill[i] = value;
        }

        // From StackOverflow fast fill question https://stackoverflow.com/questions/1897555/what-is-the-equivalent-of-memset-in-c
        public static void FillUsingBlockCopy(this byte[] array, byte value)
        {
            int block = 32, index = 0;
            int endIndex = Math.Min(block, array.Length);

            while (index < endIndex)          // Fill the initial block
                array[index++] = value;

            endIndex = array.Length;
            while (index < endIndex)
            {
                int actualBlockSize = Math.Min(block, endIndex - index);
                Buffer.BlockCopy(array, 0, array, index, actualBlockSize);
                index += block;
                block *= 2;
            }
        }
        public static void FillUsingBlockCopy(this byte[] array, byte value, int startIndex, int count)
        {
            int block = 32, index = startIndex;
            int endIndex = startIndex + Math.Min(block, count);

            while (index < endIndex)          // Fill the initial block
                array[index++] = value;

            endIndex = startIndex + count;
            while (index < endIndex)
            {
                int actualBlockSize = Math.Min(block, endIndex - index);
                Buffer.BlockCopy(array, startIndex, array, index, actualBlockSize);
                index += block;
                block *= 2;
            }
        }
        public static void FillUsingBlockCopy(this ushort[] array, ushort value)
        {
            int block = 32, index = 0;
            int length = Math.Min(block, array.Length);
            int numBytesInItem = sizeof(ushort);

            while (index < length)          // Fill the initial array
                array[index++] = value;

            length = array.Length;
            while (index < length)
            {
                int actualBlockSize = Math.Min(block, length - index);
                Buffer.BlockCopy(array, 0, array, index * numBytesInItem, actualBlockSize * numBytesInItem);
                index += block;
                block *= 2;
            }
        }
        public static void FillUsingBlockCopy(this int[] array, int value)
        {
            int block = 32, index = 0;
            int length = Math.Min(block, array.Length);
            int numBytesInItem = sizeof(int);

            while (index < length)          // Fill the initial array
                array[index++] = value;

            length = array.Length;
            while (index < length)
            {
                int actualBlockSize = Math.Min(block, length - index);
                Buffer.BlockCopy(array, 0, array, index * numBytesInItem, actualBlockSize * numBytesInItem);
                index += block;
                block *= 2;
            }
        }
    }
}
