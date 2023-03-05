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
            var context = new CoroutineContext
            {
                MaxIterations = maxIterations,
            };

            ExecuteCoroutine(coroutine, context);
        }

        private static void ExecuteCoroutine(IEnumerator coroutine, CoroutineContext context)
        {
            var moveNext = true;

            do
            {
                if (coroutine.Current is IEnumerator subCoroutine)
                {
                    ExecuteCoroutine(subCoroutine, context);
                }

                if (context.CanCoroutineContinue)
                {
                    moveNext = coroutine.MoveNext();
                    context.CurrentIterations += 1;
                }
            }
            while (moveNext && context.CanCoroutineContinue);
        }

    }


    public class CoroutineContext
    {
        public int MaxIterations { get; set; }

        public int CurrentIterations { get; set; }

        public bool CanCoroutineContinue => MaxIterations < 0 || MaxIterations > CurrentIterations;
    }
}
