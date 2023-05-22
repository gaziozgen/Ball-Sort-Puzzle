using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames.Core
{
    [Serializable]
    public abstract class VariableReference<T, U> where U : Variable<T>
    {
        public bool UseConstant = true;
        public T ConstantValue;
        public U Variable;
        public T Value
        {
            get
            {
                return UseConstant ? ConstantValue : Variable.Value;
            }
            set
            {
                if (UseConstant)
                    ConstantValue = value;
                else
                    Variable.Value = value;
            }
        }
    }
}
