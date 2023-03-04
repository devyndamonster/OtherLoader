using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Models
{
    public class ResultCoroutine<T> : IEnumerator
    {
        public T Result => _result;
        
        private readonly IEnumerator _coroutine;
        private T _result;

        public ResultCoroutine(IEnumerator coroutine)
        {
            _coroutine = coroutine;
        }

        public object Current => _coroutine.Current;

        public bool MoveNext()
        {
            if (_coroutine.Current is T result)
            {
                _result = result;
            }

            return _coroutine.MoveNext();
        }

        public void Reset()
        {
            _coroutine.Reset();
        }
    }
}
