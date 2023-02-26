using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Helpers
{
    public static class CoroutineExtensions
    {
        public static void ExecuteCoroutine(this IEnumerator coroutine)
        {
            var moveNext = true;

            do
            {
                if (coroutine.Current is IEnumerator subCoroutine)
                {
                    subCoroutine.ExecuteCoroutine();
                }
                
                moveNext = coroutine.MoveNext();
            }
            while (moveNext);
        }

    }
}
