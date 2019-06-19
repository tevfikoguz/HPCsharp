﻿// TODO: Implement not just Zero detection, but detection of any constant value, specified by the user.
// TODO: Implement constant detection - detect whether the array is a constant or not.
// TODO: Implement multi-core zero detection using the best SSE algorithm.
// TODO: Improve implementation of equal of byte[] and other data types using the same technique and post to https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net?noredirect=1&lq=1
//       once memory bandwidth limit has been reached by the implementation. This is the same type of a problem, where AND condition is needed.
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Threading.Tasks;

namespace HPCsharp.ParallelAlgorithms
{
    static public partial class ZeroDetect
    {
        private static bool ZeroDetectSseInner(this byte[] arrayToOr, int l, int r)
        {
            var orVector   = new Vector<byte>(0);
            var ZeroVector = new Vector<byte>(0);
            int sseIndexEnd = l + ((r - l + 1) / Vector<byte>.Count) * Vector<byte>.Count;
            int i;
            for (i = l; i < sseIndexEnd; i += Vector<byte>.Count)
            {
                var inVector = new Vector<byte>(arrayToOr, i);
                orVector |= inVector;
                if (!Vector.EqualsAll(inVector, ZeroVector))
                    return false;
            }
            byte overallOr = 0;
            for (; i <= r; i++)
                overallOr |= arrayToOr[i];
            for (i = 0; i < Vector<byte>.Count; i++)
                overallOr |= orVector[i];
            return overallOr == 0;
        }

        public static bool ZeroDetectSse(this byte[] arrayToDetect)
        {
            return arrayToDetect.ZeroDetectSseInner(0, arrayToDetect.Length - 1);
        }

        // Pattern for unrolling to any amount, but doesn't gain much yet, until CPUs execute more instructions in parallel without dependencies.
        // May benefit from separate/independent increments.
        private static bool ZeroDetectSseInner2(this byte[] arrayToOr, int l, int r)
        {
            var orVector   = new Vector<byte>(0);
            var ZeroVector = new Vector<byte>(0);
            int numVectorsPerInnerLoop = 128;
            int numVectorsConcurrently = 4;
            int offset1 = Vector<byte>.Count;
            int offset2 = Vector<byte>.Count * 2;
            int offset3 = Vector<byte>.Count * 3;
            int numElementsPerInnerLoop = numVectorsPerInnerLoop * numVectorsConcurrently * Vector<byte>.Count;
            int sseIndexEnd = l + ((r - l + 1) / numElementsPerInnerLoop) * numElementsPerInnerLoop;
            int i;
            for (i = l; i < sseIndexEnd; i += numElementsPerInnerLoop)
            {
                int currLoopEnd = i + numElementsPerInnerLoop;
                int innerLoopIncrement = Vector<byte>.Count * numVectorsConcurrently;
                for (int j = i; j < currLoopEnd; j += innerLoopIncrement)
                {
                    orVector |= new Vector<byte>(arrayToOr, j);
                    orVector |= new Vector<byte>(arrayToOr, j + offset1);
                    orVector |= new Vector<byte>(arrayToOr, j + offset2);
                    orVector |= new Vector<byte>(arrayToOr, j + offset3);
                }
                if (orVector != ZeroVector)
                    return false;
            }
            byte overallOr = 0;
            for (; i <= r; i++)
                overallOr |= arrayToOr[i];
            for (i = 0; i < Vector<byte>.Count; i++)
                overallOr |= orVector[i];
            return overallOr == 0;
        }

        private static bool ZeroDetectSse2(this byte[] arrayToDetect)
        {
            return arrayToDetect.ZeroDetectSseInner2(0, arrayToDetect.Length - 1);
        }

        private static bool ZeroDetectSseUnrolledInner(this byte[] arrayToOr, int l, int r)
        {
            var zeroVector = new Vector<byte>(0);
            int concurrentAmount = 4;
            int sseIndexEnd = l + ((r - l + 1) / (Vector<byte>.Count * concurrentAmount)) * (Vector<byte>.Count * concurrentAmount);
            int i;
            int offset1 = Vector<byte>.Count;
            int offset2 = Vector<byte>.Count * 2;
            int offset3 = Vector<byte>.Count * 3;
            int increment = Vector<byte>.Count * concurrentAmount;
            for (i = l; i < sseIndexEnd; i += increment)
            {
                var inVector  = new Vector<byte>(arrayToOr, i          );
                inVector     |= new Vector<byte>(arrayToOr, i + offset1);
                inVector     |= new Vector<byte>(arrayToOr, i + offset2);
                inVector     |= new Vector<byte>(arrayToOr, i + offset3);
                if (!Vector.EqualsAll(inVector, zeroVector))
                    return false;
            }
            byte overallOr = 0;
            for (; i <= r; i++)
                overallOr |= arrayToOr[i];
            return overallOr == 0;
        }

        public static bool ZeroDetectSseUnrolled(this byte[] arrayToDetect)
        {
            return arrayToDetect.ZeroDetectSseUnrolledInner(0, arrayToDetect.Length - 1);
        }

        public static bool ZeroDetectSseUnrolled(this byte[] arrayToProcess, int start, int length, int thresholdParallel = 4096)
        {
            return arrayToProcess.ZeroDetectSseUnrolledInner(start, start + length - 1);
        }

        private static bool ZeroDetectSseUnrolledInner2(this byte[] arrayToOr, int l, int r)
        {
            var orVector0 = new Vector<byte>(0);
            var orVector1 = new Vector<byte>(0);
            var orVector2 = new Vector<byte>(0);
            var orVector3 = new Vector<byte>(0);
            int concurrentAmount = 4;
            int sseIndexEnd = l + ((r - l + 1) / (Vector<byte>.Count * concurrentAmount)) * (Vector<byte>.Count * concurrentAmount);
            int i;
            int offset1 = Vector<byte>.Count;
            int offset2 = Vector<byte>.Count * 2;
            int offset3 = Vector<byte>.Count * 3;
            int increment = Vector<byte>.Count * concurrentAmount;
            for (i = l; i < sseIndexEnd; i += increment)
            {
                var inVector0 = new Vector<byte>(arrayToOr, i);
                var inVector1 = new Vector<byte>(arrayToOr, i + offset1);
                var inVector2 = new Vector<byte>(arrayToOr, i + offset2);
                var inVector3 = new Vector<byte>(arrayToOr, i + offset3);
                orVector0 |= inVector0;
                orVector1 |= inVector1;
                orVector2 |= inVector2;
                orVector3 |= inVector3;
            }
            byte overallOr = 0;
            for (; i <= r; i++)
                overallOr |= arrayToOr[i];
            orVector0 |= orVector1;
            orVector0 |= orVector2;
            orVector0 |= orVector3;
            for (i = 0; i < Vector<byte>.Count; i++)
                overallOr |= orVector0[i];
            return overallOr == 0;
        }

        private static bool ZeroDetectSseUnrolled2(this byte[] arrayToDetect)
        {
            return arrayToDetect.ZeroDetectSseUnrolledInner2(0, arrayToDetect.Length - 1);
        }

        private static bool ZeroDetectSseUnrolledParInner(this byte[] arrayToProcess, int l, int r, int thresholdParallel = 4096)
        {
            bool partLeft = false;

            if (l > r)
                return partLeft;
            if ((r - l + 1) <= thresholdParallel)
                return ZeroDetectSseUnrolledInner(arrayToProcess, l, r - l + 1);

            int m = (r + l) / 2;

            bool partRight = false;

            Parallel.Invoke(
                () => { partLeft  = ZeroDetectSseUnrolledParInner(arrayToProcess, l,     m); },
                () => { partRight = ZeroDetectSseUnrolledParInner(arrayToProcess, m + 1, r); }
            );
            // Combine left and right results
            return partLeft && partRight;
        }

        public static bool ZeroDetectSseUnrolledPar(this byte[] arrayToProcess, int thresholdParallel = 4096)
        {
            return ZeroDetectSseUnrolledParInner(arrayToProcess, 0, arrayToProcess.Length - 1, thresholdParallel);
        }

        public static bool ZeroDetectSseUnrolledPar(this byte[] arrayToProcess, int start, int length, int thresholdParallel = 4096)
        {
            return arrayToProcess.ZeroDetectSseUnrolledParInner(start, start + length - 1, thresholdParallel);
        }

        private static bool ZeroDetectSseParInner(this byte[] arrayToProcess, int l, int r, int thresholdParallel = 4096)
        {
            bool partLeft = false;

            if (l > r)
                return partLeft;
            if ((r - l + 1) <= thresholdParallel)
                return ZeroDetectSseInner(arrayToProcess, l, r - l + 1);

            int m = (r + l) / 2;

            bool partRight = false;

            Parallel.Invoke(
                () => { partLeft  = ZeroDetectSseParInner(arrayToProcess, l, m); },
                () => { partRight = ZeroDetectSseParInner(arrayToProcess, m + 1, r); }
            );
            // Combine left and right results
            return partLeft && partRight;
        }

        public static bool ZeroDetectSsePar(this byte[] arrayToProcess, int thresholdParallel = 4096)
        {
            return ZeroDetectSseParInner(arrayToProcess, 0, arrayToProcess.Length - 1, thresholdParallel);
        }

        public static bool ZeroDetectSsePar(this byte[] arrayToProcess, int start, int length, int thresholdParallel = 4096)
        {
            return arrayToProcess.ZeroDetectSseParInner(start, start + length - 1, thresholdParallel);
        }

        private static bool ZeroDetectUnrolledParInner(this byte[] arrayToProcess, int l, int r, int thresholdParallel = 4096)
        {
            bool partLeft = false;

            if (l > r)
                return partLeft;
            if ((r - l + 1) <= thresholdParallel)
                return HPCsharp.ZeroDetect.ByFixedLongUnrolled(arrayToProcess, l, r - l + 1);

            int m = (((r + l) / 64 ) * 64) / 2;

            bool partRight = false;

            Parallel.Invoke(
                () => { partLeft  = ZeroDetectSseUnrolledParInner(arrayToProcess, l,     m); },
                () => { partRight = ZeroDetectSseUnrolledParInner(arrayToProcess, m + 1, r); }
            );
            // Combine left and right results
            return partLeft && partRight;
        }

        public static bool ZeroDetectUnrolledPar(this byte[] arrayToProcess, int thresholdParallel = 4096)
        {
            return ZeroDetectUnrolledParInner(arrayToProcess, 0, arrayToProcess.Length - 1, thresholdParallel);
        }

        public static bool ZeroDetectUnrolledPar(this byte[] arrayToProcess, int start, int length, int thresholdParallel = 4096)
        {
            return arrayToProcess.ZeroDetectUnrolledParInner(start, start + length - 1, thresholdParallel);
        }

#if false
        // TODO: Need to figure out how to implement this with Vector pointers, as these don't seem to be available in .NET standard yet, but maybe in Core?
        static unsafe bool BySimdUnrolled(byte[] data)
        {
            fixed (byte* bytes = data)
            {
                int len = data.Length;
                int rem = len % (16 * 16);
                Vector16b* b = (Vector16b*)bytes;
                Vector16b* e = (Vector16b*)(bytes + len - rem);
                Vector16b zero = Vector16b.Zero;

                while (b < e)
                {
                    if ((*(b) | *(b + 1) | *(b + 2) | *(b + 3) | *(b + 4) |
                        *(b + 5) | *(b + 6) | *(b + 7) | *(b + 8) |
                        *(b + 9) | *(b + 10) | *(b + 11) | *(b + 12) |
                        *(b + 13) | *(b + 14) | *(b + 15)) != zero)
                        return false;
                    b += 16;
                }

                for (int i = 0; i < rem; i++)
                    if (data[len - 1 - i] != 0)
                        return false;

                return true;
            }
        }
#endif
    }
}