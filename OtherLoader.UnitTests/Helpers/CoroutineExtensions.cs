using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Helpers
{
    public static class CoroutineExtensions
    {
        public static void ExecuteCoroutine(this IEnumerator coroutine, int maxIterations = -1)
        {
            var moveNext = true;
            var iterations = 0;

            do
            {
                if (coroutine.Current is IEnumerator subCoroutine)
                {
                    subCoroutine.ExecuteCoroutine(maxIterations);
                }

                moveNext = coroutine.MoveNext();
                iterations += 1;
            }
            while (moveNext && (maxIterations < 0 || maxIterations > iterations));
        }

    }
}
