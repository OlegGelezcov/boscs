using System;
using System.Collections.Generic;
/*
[Serializable]
public class BigInt
{
    #region Fields

    private DigitsArray _digits;

    #endregion

    #region Properties

    public bool IsNegative { get { return _digits.IsNegative; } }
    public bool IsZero { get { return _digits.IsZero; } }

    #endregion

    #region Constructors

    public BigInt()
    {
        _digits = new DigitsArray(1, 1);
    }
    public BigInt(double number)
        : this(Convert.ToInt64(number))
    {

    }
    public BigInt(long number)
    {
        _digits = new DigitsArray((8 / DigitsArray.DataSizeOf) + 1, 0);
        while (number != 0 && _digits.DataUsed < _digits.Count)
        {
            _digits[_digits.DataUsed] = (uint)(number & DigitsArray._allBits);
            number >>= DigitsArray.DataSizeBits;
            _digits.DataUsed++;
        }
        _digits.ResetDataUsed();
    }
    public BigInt(ulong number)
    {
        _digits = new DigitsArray((8 / DigitsArray.DataSizeOf) + 1, 0);
        while (number != 0 && _digits.DataUsed < _digits.Count)
        {
            _digits[_digits.DataUsed] = (uint)(number & DigitsArray._allBits);
            number >>= DigitsArray.DataSizeBits;
            _digits.DataUsed++;
        }
        _digits.ResetDataUsed();
    }
    public BigInt(string digits)
    {
        Construct(digits, 10);
    }
    public BigInt(byte[] array)
    {
        ConstructFrom(array, 0, array.Length);
    }
    public BigInt(byte[] array, int length)
    {
        ConstructFrom(array, 0, length);
    }
    public BigInt(byte[] array, int offset, int length)
    {
        ConstructFrom(array, offset, length);
    }
    public BigInt(string digits, int radix)
    {
        Construct(digits, radix);
    }

    private BigInt(DigitsArray digits)
    {
        digits.ResetDataUsed();
        _digits = digits;
    }
    private void Construct(string digits, int radix)
    {
        if (digits == null)
        {
            throw new ArgumentNullException("digits");
        }

        BigInt multiplier = new BigInt(1);
        BigInt result = new BigInt();
        digits = digits.ToUpper(System.Globalization.CultureInfo.CurrentCulture).Trim();

        int nDigits = (digits[0] == '-' ? 1 : 0);

        for (int idx = digits.Length - 1; idx >= nDigits; idx--)
        {
            int d = (int)digits[idx];
            if (d >= '0' && d <= '9')
            {
                d -= '0';
            }
            else if (d >= 'A' && d <= 'Z')
            {
                d = (d - 'A') + 10;
            }
            else
            {
                throw new ArgumentOutOfRangeException("digits");
            }

            if (d >= radix)
            {
                throw new ArgumentOutOfRangeException("digits");
            }
            result += (multiplier * d);
            multiplier *= radix;
        }

        if (digits[0] == '-')
        {
            result = -result;
        }

        this._digits = result._digits;
    }
    private void ConstructFrom(byte[] array, int offset, int length)
    {
        if (array == null)
        {
            throw new ArgumentNullException("array");
        }
        if (offset > array.Length || length > array.Length)
        {
            throw new ArgumentOutOfRangeException("offset");
        }
        if (length > array.Length || (offset + length) > array.Length)
        {
            throw new ArgumentOutOfRangeException("length");
        }

        int estSize = length / 4;
        int leftOver = length & 3;
        if (leftOver != 0)
        {
            ++estSize;
        }

        _digits = new DigitsArray(estSize + 1, 0); // alloc one extra since we can't init -'s from here.

        for (int i = offset + length - 1, j = 0; (i - offset) >= 3; i -= 4, j++)
        {
            _digits[j] = (uint)((array[i - 3] << 24) + (array[i - 2] << 16) + (array[i - 1] << 8) + array[i]);
            _digits.DataUsed++;
        }

        uint accumulator = 0;
        for (int i = leftOver; i > 0; i--)
        {
            uint digit = array[offset + leftOver - i];
            digit = (digit << ((i - 1) * 8));
            accumulator |= digit;
        }
        _digits[_digits.DataUsed] = accumulator;

        _digits.ResetDataUsed();
    }

    #endregion

    #region Cast Operators

    public static implicit operator BigInt(long value)
    {
        return (new BigInt(value));
    }
    public static implicit operator BigInt(ulong value)
    {
        return (new BigInt(value));
    }
    public static implicit operator BigInt(int value)
    {
        return (new BigInt((long)value));
    }
    public static implicit operator BigInt(uint value)
    {
        return (new BigInt((ulong)value));
    }

    #endregion

    #region Math Operators

    public static BigInt operator +(BigInt leftSide, BigInt rightSide)
    {
        int size = Math.Max(leftSide._digits.DataUsed, rightSide._digits.DataUsed);
        DigitsArray da = new DigitsArray(size + 1);

        long carry = 0;
        for (int i = 0; i < da.Count; i++)
        {
            long sum = (long)leftSide._digits[i] + (long)rightSide._digits[i] + carry;
            carry = (long)(sum >> DigitsArray.DataSizeBits);
            da[i] = (uint)(sum & DigitsArray._allBits);
        }

        return new BigInt(da);
    }
    public static BigInt operator ++(BigInt leftSide)
    {
        return (leftSide + 1);
    }

    public static BigInt operator -(BigInt leftSide, BigInt rightSide)
    {
        int size = System.Math.Max(leftSide._digits.DataUsed, rightSide._digits.DataUsed) + 1;
        DigitsArray da = new DigitsArray(size);

        long carry = 0;
        for (int i = 0; i < da.Count; i++)
        {
            long diff = (long)leftSide._digits[i] - (long)rightSide._digits[i] - carry;
            da[i] = (uint)(diff & DigitsArray._allBits);
            da.DataUsed++;
            carry = ((diff < 0) ? 1 : 0);
        }
        return new BigInt(da);
    }
    public static BigInt operator -(BigInt leftSide)
    {
        if (object.ReferenceEquals(leftSide, null))
        {
            throw new ArgumentNullException("leftSide");
        }

        if (leftSide.IsZero)
        {
            return new BigInt(0);
        }

        DigitsArray da = new DigitsArray(leftSide._digits.DataUsed + 1, leftSide._digits.DataUsed + 1);

        for (int i = 0; i < da.Count; i++)
        {
            da[i] = (uint)(~(leftSide._digits[i]));
        }

        // add one to result (1's complement + 1)
        bool carry = true;
        int index = 0;
        while (carry && index < da.Count)
        {
            long val = (long)da[index] + 1;
            da[index] = (uint)(val & DigitsArray._allBits);
            carry = ((val >> DigitsArray.DataSizeBits) > 0);
            index++;
        }

        return new BigInt(da);
    }
    public static BigInt operator --(BigInt leftSide)
    {
        return (leftSide - 1);
    }

    public static BigInt operator *(BigInt leftSide, BigInt rightSide)
    {
        if (object.ReferenceEquals(leftSide, null))
        {
            throw new ArgumentNullException("leftSide");
        }
        if (object.ReferenceEquals(rightSide, null))
        {
            throw new ArgumentNullException("rightSide");
        }

        bool leftSideNeg = leftSide.IsNegative;
        bool rightSideNeg = rightSide.IsNegative;

        leftSide = Abs(leftSide);
        rightSide = Abs(rightSide);

        DigitsArray da = new DigitsArray(leftSide._digits.DataUsed + rightSide._digits.DataUsed);
        da.DataUsed = da.Count;

        for (int i = 0; i < leftSide._digits.DataUsed; i++)
        {
            ulong carry = 0;
            for (int j = 0, k = i; j < rightSide._digits.DataUsed; j++, k++)
            {
                ulong val = ((ulong)leftSide._digits[i] * (ulong)rightSide._digits[j]) + (ulong)da[k] + carry;

                da[k] = (uint)(val & DigitsArray._allBits);
                carry = (val >> DigitsArray.DataSizeBits);
            }

            if (carry != 0)
            {
                da[i + rightSide._digits.DataUsed] = (uint)carry;
            }
        }

        //da.ResetDataUsed();
        BigInt result = new BigInt(da);
        return (leftSideNeg != rightSideNeg ? -result : result);
    }
    public static BigInt operator /(BigInt leftSide, BigInt rightSide)
    {
        if (leftSide == null)
        {
            throw new ArgumentNullException("leftSide");
        }
        if (rightSide == null)
        {
            throw new ArgumentNullException("rightSide");
        }

        if (rightSide.IsZero)
        {
            throw new DivideByZeroException();
        }

        bool divisorNeg = rightSide.IsNegative;
        bool dividendNeg = leftSide.IsNegative;

        leftSide = Abs(leftSide);
        rightSide = Abs(rightSide);

        if (leftSide < rightSide)
        {
            return new BigInt(0);
        }

        BigInt quotient;
        BigInt remainder;
        Divide(leftSide, rightSide, out quotient, out remainder);

        return (dividendNeg != divisorNeg ? -quotient : quotient);
    }
    public static BigInt operator %(BigInt leftSide, BigInt rightSide)
    {
        if (leftSide == null)
        {
            throw new ArgumentNullException("leftSide");
        }

        if (rightSide == null)
        {
            throw new ArgumentNullException("rightSide");
        }

        if (rightSide.IsZero)
        {
            throw new DivideByZeroException();
        }

        BigInt quotient;
        BigInt remainder;

        bool dividendNeg = leftSide.IsNegative;
        leftSide = Abs(leftSide);
        rightSide = Abs(rightSide);

        if (leftSide < rightSide)
        {
            return leftSide;
        }

        Divide(leftSide, rightSide, out quotient, out remainder);

        return (dividendNeg ? -remainder : remainder);
    }

    public static BigInt operator &(BigInt leftSide, BigInt rightSide)
    {
        int len = System.Math.Max(leftSide._digits.DataUsed, rightSide._digits.DataUsed);
        DigitsArray da = new DigitsArray(len, len);
        for (int idx = 0; idx < len; idx++)
        {
            da[idx] = (uint)(leftSide._digits[idx] & rightSide._digits[idx]);
        }
        return new BigInt(da);
    }
    public static BigInt operator |(BigInt leftSide, BigInt rightSide)
    {
        int len = System.Math.Max(leftSide._digits.DataUsed, rightSide._digits.DataUsed);
        DigitsArray da = new DigitsArray(len, len);
        for (int idx = 0; idx < len; idx++)
        {
            da[idx] = (uint)(leftSide._digits[idx] | rightSide._digits[idx]);
        }
        return new BigInt(da);
    }
    public static BigInt operator ^(BigInt leftSide, BigInt rightSide)
    {
        int len = System.Math.Max(leftSide._digits.DataUsed, rightSide._digits.DataUsed);
        DigitsArray da = new DigitsArray(len, len);
        for (int idx = 0; idx < len; idx++)
        {
            da[idx] = (uint)(leftSide._digits[idx] ^ rightSide._digits[idx]);
        }
        return new BigInt(da);
    }
    public static BigInt operator ~(BigInt leftSide)
    {
        DigitsArray da = new DigitsArray(leftSide._digits.Count);
        for (int idx = 0; idx < da.Count; idx++)
        {
            da[idx] = (uint)(~(leftSide._digits[idx]));
        }

        return new BigInt(da);
    }

    public static BigInt operator <<(BigInt leftSide, int shiftCount)
    {
        if (leftSide == null)
        {
            throw new ArgumentNullException("leftSide");
        }

        DigitsArray da = new DigitsArray(leftSide._digits);
        da.DataUsed = da.ShiftLeftWithoutOverflow(shiftCount);

        return new BigInt(da);
    }
    public static BigInt operator >>(BigInt leftSide, int shiftCount)
    {
        if (leftSide == null)
        {
            throw new ArgumentNullException("leftSide");
        }

        DigitsArray da = new DigitsArray(leftSide._digits);
        da.DataUsed = da.ShiftRight(shiftCount);

        if (leftSide.IsNegative)
        {
            for (int i = da.Count - 1; i >= da.DataUsed; i--)
            {
                da[i] = DigitsArray._allBits;
            }

            uint mask = DigitsArray._hiBitSet;
            for (int i = 0; i < DigitsArray.DataSizeBits; i++)
            {
                if ((da[da.DataUsed - 1] & mask) == DigitsArray._hiBitSet)
                {
                    break;
                }
                da[da.DataUsed - 1] |= mask;
                mask >>= 1;
            }
            da.DataUsed = da.Count;
        }

        return new BigInt(da);
    }

    #endregion

    #region Math

    public static BigInt Add(BigInt leftSide, BigInt rightSide)
    {
        return leftSide + rightSide;
    }
    public static BigInt Subtract(BigInt leftSide, BigInt rightSide)
    {
        return leftSide - rightSide;
    }
    public static BigInt Multiply(BigInt leftSide, BigInt rightSide)
    {
        return leftSide * rightSide;
    }
    public static BigInt Divide(BigInt leftSide, BigInt rightSide)
    {
        return leftSide / rightSide;
    }
    public static BigInt Modulus(BigInt leftSide, BigInt rightSide)
    {
        return leftSide % rightSide;
    }
    public static BigInt Increment(BigInt leftSide)
    {
        return (leftSide + 1);
    }
    public static BigInt Decrement(BigInt leftSide)
    {
        return (leftSide - 1);
    }
    public static BigInt Abs(BigInt leftSide)
    {
        if (object.ReferenceEquals(leftSide, null))
        {
            throw new ArgumentNullException("leftSide");
        }
        if (leftSide.IsNegative)
        {
            return -leftSide;
        }
        return leftSide;
    }
    public BigInt Negate()
    {
        return -this;
    }
    public static BigInt BitwiseAnd(BigInt leftSide, BigInt rightSide)
    {
        return leftSide & rightSide;
    }
    public static BigInt BitwiseOr(BigInt leftSide, BigInt rightSide)
    {
        return leftSide | rightSide;
    }
    public static BigInt Xor(BigInt leftSide, BigInt rightSide)
    {
        return leftSide ^ rightSide;
    }
    public static BigInt OnesComplement(BigInt leftSide)
    {
        return ~leftSide;
    }
    public static BigInt LeftShift(BigInt leftSide, int shiftCount)
    {
        return leftSide << shiftCount;
    }
    public static BigInt RightShift(BigInt leftSide, int shiftCount)
    {
        if (leftSide == null)
        {
            throw new ArgumentNullException("leftSide");
        }

        return leftSide >> shiftCount;
    }
    #endregion

    #region Math Helpers

    public static void Divide(BigInt leftSide, BigInt rightSide, out BigInt quotient, out BigInt remainder)
    {
        if (leftSide.IsZero)
        {
            quotient = new BigInt();
            remainder = new BigInt();
            return;
        }

        if (rightSide._digits.DataUsed == 1)
        {
            SingleDivide(leftSide, rightSide, out quotient, out remainder);
        }
        else
        {
            MultiDivide(leftSide, rightSide, out quotient, out remainder);
        }
    }
    private static void MultiDivide(BigInt leftSide, BigInt rightSide, out BigInt quotient, out BigInt remainder)
    {
        if (rightSide.IsZero)
        {
            throw new DivideByZeroException();
        }

        uint val = rightSide._digits[rightSide._digits.DataUsed - 1];
        int d = 0;
        for (uint mask = DigitsArray._hiBitSet; mask != 0 && (val & mask) == 0; mask >>= 1)
        {
            d++;
        }

        int remainderLen = leftSide._digits.DataUsed + 1;
        uint[] remainderDat = new uint[remainderLen];
        leftSide._digits.CopyTo(remainderDat, 0, leftSide._digits.DataUsed);

        DigitsArray.ShiftLeft(remainderDat, d);
        rightSide = rightSide << d;

        ulong firstDivisor = rightSide._digits[rightSide._digits.DataUsed - 1];
        ulong secondDivisor = (rightSide._digits.DataUsed < 2 ? (uint)0 : rightSide._digits[rightSide._digits.DataUsed - 2]);

        int divisorLen = rightSide._digits.DataUsed + 1;
        DigitsArray dividendPart = new DigitsArray(divisorLen, divisorLen);
        uint[] result = new uint[leftSide._digits.Count + 1];
        int resultPos = 0;

        ulong carryBit = (ulong)0x1 << DigitsArray.DataSizeBits; // 0x100000000
        for (int j = remainderLen - rightSide._digits.DataUsed, pos = remainderLen - 1; j > 0; j--, pos--)
        {
            ulong dividend = ((ulong)remainderDat[pos] << DigitsArray.DataSizeBits) + (ulong)remainderDat[pos - 1];
            ulong qHat = (dividend / firstDivisor);
            ulong rHat = (dividend % firstDivisor);

            while (pos >= 2)
            {
                if (qHat == carryBit || (qHat * secondDivisor) > ((rHat << DigitsArray.DataSizeBits) + remainderDat[pos - 2]))
                {
                    qHat--;
                    rHat += firstDivisor;
                    if (rHat < carryBit)
                    {
                        continue;
                    }
                }
                break;
            }

            for (int h = 0; h < divisorLen; h++)
            {
                dividendPart[divisorLen - h - 1] = remainderDat[pos - h];
            }

            BigInt dTemp = new BigInt(dividendPart);
            BigInt rTemp = rightSide * (long)qHat;
            while (rTemp > dTemp)
            {
                qHat--;
                rTemp -= rightSide;
            }

            rTemp = dTemp - rTemp;
            for (int h = 0; h < divisorLen; h++)
            {
                remainderDat[pos - h] = rTemp._digits[rightSide._digits.DataUsed - h];
            }

            result[resultPos++] = (uint)qHat;
        }

        Array.Reverse(result, 0, resultPos);
        quotient = new BigInt(new DigitsArray(result));

        int n = DigitsArray.ShiftRight(remainderDat, d);
        DigitsArray rDA = new DigitsArray(n, n);
        rDA.CopyFrom(remainderDat, 0, 0, rDA.DataUsed);
        remainder = new BigInt(rDA);
    }
    private static void SingleDivide(BigInt leftSide, BigInt rightSide, out BigInt quotient, out BigInt remainder)
    {
        if (rightSide.IsZero)
        {
            throw new DivideByZeroException();
        }

        DigitsArray remainderDigits = new DigitsArray(leftSide._digits);
        remainderDigits.ResetDataUsed();

        int pos = remainderDigits.DataUsed - 1;
        ulong divisor = (ulong)rightSide._digits[0];
        ulong dividend = (ulong)remainderDigits[pos];

        uint[] result = new uint[leftSide._digits.Count];
        leftSide._digits.CopyTo(result, 0, result.Length);
        int resultPos = 0;

        if (dividend >= divisor)
        {
            result[resultPos++] = (uint)(dividend / divisor);
            remainderDigits[pos] = (uint)(dividend % divisor);
        }
        pos--;

        while (pos >= 0)
        {
            dividend = ((ulong)(remainderDigits[pos + 1]) << DigitsArray.DataSizeBits) + (ulong)remainderDigits[pos];
            result[resultPos++] = (uint)(dividend / divisor);
            remainderDigits[pos + 1] = 0;
            remainderDigits[pos--] = (uint)(dividend % divisor);
        }
        remainder = new BigInt(remainderDigits);

        DigitsArray quotientDigits = new DigitsArray(resultPos + 1, resultPos);
        int j = 0;
        for (int i = quotientDigits.DataUsed - 1; i >= 0; i--, j++)
        {
            quotientDigits[j] = result[i];
        }
        quotient = new BigInt(quotientDigits);
    }

    #endregion


    #region Relational Operators
   
    public static bool operator ==(BigInt leftSide, BigInt rightSide)
    {
        if (object.ReferenceEquals(leftSide, rightSide))
        {
            return true;
        }

        if (object.ReferenceEquals(leftSide, null) || object.ReferenceEquals(rightSide, null))
        {
            return false;
        }

        if (leftSide.IsNegative != rightSide.IsNegative)
        {
            return false;
        }

        return leftSide.Equals(rightSide);
    }
    public static bool operator !=(BigInt leftSide, BigInt rightSide)
    {
        return !(leftSide == rightSide);
    }
    public static bool operator >(BigInt leftSide, BigInt rightSide)
    {
        if (object.ReferenceEquals(leftSide, null))
        {
            throw new ArgumentNullException("leftSide");
        }

        if (object.ReferenceEquals(rightSide, null))
        {
            throw new ArgumentNullException("rightSide");
        }

        if (leftSide.IsNegative != rightSide.IsNegative)
        {
            return rightSide.IsNegative;
        }

        if (leftSide._digits.DataUsed != rightSide._digits.DataUsed)
        {
            return leftSide._digits.DataUsed > rightSide._digits.DataUsed;
        }

        for (int idx = leftSide._digits.DataUsed - 1; idx >= 0; idx--)
        {
            if (leftSide._digits[idx] != rightSide._digits[idx])
            {
                return (leftSide._digits[idx] > rightSide._digits[idx]);
            }
        }
        return false;
    }
    public static bool operator <(BigInt leftSide, BigInt rightSide)
    {
        if (object.ReferenceEquals(leftSide, null))
        {
            throw new ArgumentNullException("leftSide");
        }

        if (object.ReferenceEquals(rightSide, null))
        {
            throw new ArgumentNullException("rightSide");
        }

        if (leftSide.IsNegative != rightSide.IsNegative)
        {
            return leftSide.IsNegative;
        }

        if (leftSide._digits.DataUsed != rightSide._digits.DataUsed)
        {
            return leftSide._digits.DataUsed < rightSide._digits.DataUsed;
        }

        for (int idx = leftSide._digits.DataUsed - 1; idx >= 0; idx--)
        {
            if (leftSide._digits[idx] != rightSide._digits[idx])
            {
                return (leftSide._digits[idx] < rightSide._digits[idx]);
            }
        }
        return false;
    }
    public static bool operator >=(BigInt leftSide, BigInt rightSide)
    {
        return Compare(leftSide, rightSide) >= 0;
    }
    public static bool operator <=(BigInt leftSide, BigInt rightSide)
    {
        return Compare(leftSide, rightSide) <= 0;
    }

    public int CompareTo(BigInt value)
    {
        return Compare(this, value);
    }
    public static int Compare(BigInt leftSide, BigInt rightSide)
    {
        if (object.ReferenceEquals(leftSide, rightSide))
        {
            return 0;
        }

        if (object.ReferenceEquals(leftSide, null))
        {
            throw new ArgumentNullException("leftSide");
        }

        if (object.ReferenceEquals(rightSide, null))
        {
            throw new ArgumentNullException("rightSide");
        }

        if (leftSide > rightSide) return 1;
        if (leftSide == rightSide) return 0;
        return -1;
    }

    #endregion

   
    public override bool Equals(object obj)
    {
        if (object.ReferenceEquals(obj, null))
        {
            return false;
        }

        if (object.ReferenceEquals(this, obj))
        {
            return true;
        }

        BigInt c = (BigInt)obj;
        if (this._digits.DataUsed != c._digits.DataUsed)
        {
            return false;
        }

        for (int idx = 0; idx < this._digits.DataUsed; idx++)
        {
            if (this._digits[idx] != c._digits[idx])
            {
                return false;
            }
        }
        return true;
    }
    public override int GetHashCode()
    {
        return this._digits.GetHashCode();
    }
    public override string ToString()
    {
        return ToString(10);
    }

    #region Type Conversion
   
    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36)
        {
            throw new ArgumentOutOfRangeException("radix");
        }

        if (IsZero)
        {
            return "0";
        }

        BigInt a = this;
        bool negative = a.IsNegative;
        a = Abs(this);

        BigInt quotient;
        BigInt remainder;
        BigInt biRadix = new BigInt(radix);

        const string charSet = "0123456789abcdefghijklmnopqrstuvwxyz";
        System.Collections.ArrayList al = new System.Collections.ArrayList();
        while (a._digits.DataUsed > 1 || (a._digits.DataUsed == 1 && a._digits[0] != 0))
        {
            Divide(a, biRadix, out quotient, out remainder);
            al.Insert(0, charSet[(int)remainder._digits[0]]);
            a = quotient;
        }

        string result = new String((char[])al.ToArray(typeof(char)));
        if (radix == 10 && negative)
        {
            return "-" + result;
        }

        return result;
    }
    public string ToHexString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendFormat("{0:X}", _digits[_digits.DataUsed - 1]);

        string f = "{0:X" + (2 * DigitsArray.DataSizeOf) + "}";
        for (int i = _digits.DataUsed - 2; i >= 0; i--)
        {
            sb.AppendFormat(f, _digits[i]);
        }

        return sb.ToString();
    }
   
    public static int ToInt16(BigInt value)
    {
        if (object.ReferenceEquals(value, null))
        {
            throw new ArgumentNullException("value");
        }
        return System.Int16.Parse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture);
    }
    public static uint ToUInt16(BigInt value)
    {
        if (object.ReferenceEquals(value, null))
        {
            throw new ArgumentNullException("value");
        }
        return System.UInt16.Parse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture);
    }
    public static int ToInt32(BigInt value)
    {
        if (object.ReferenceEquals(value, null))
        {
            throw new ArgumentNullException("value");
        }
        return System.Int32.Parse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture);
    }
    public static uint ToUInt32(BigInt value)
    {
        if (object.ReferenceEquals(value, null))
        {
            throw new ArgumentNullException("value");
        }
        return System.UInt32.Parse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture);
    }
    public static long ToInt64(BigInt value)
    {
        if (object.ReferenceEquals(value, null))
        {
            throw new ArgumentNullException("value");
        }
        return System.Int64.Parse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture);
    }
    public static ulong ToUInt64(BigInt value)
    {
        if (object.ReferenceEquals(value, null))
        {
            throw new ArgumentNullException("value");
        }
        return System.UInt64.Parse(value.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.CurrentCulture);
    }

    #endregion
}

public class DigitsArray
{
    private uint[] _data;
    private int _dataUsed;

    internal static readonly uint _allBits;     // = ~((uint)0);
    internal static readonly uint _hiBitSet;    // = 0x80000000;
    internal static int DataSizeOf
    {
        get { return sizeof(uint); }
    }

    internal static int DataSizeBits
    {
        get { return sizeof(uint) * 8; }
    }

    internal int DataUsed
    {
        get { return _dataUsed; }
        set { _dataUsed = value; }
    }

    internal DigitsArray(int size)
    {
        Allocate(size, 0);
    }

    internal DigitsArray(int size, int used)
    {
        Allocate(size, used);
    }

    internal DigitsArray(uint[] copyFrom)
    {
        Allocate(copyFrom.Length);
        CopyFrom(copyFrom, 0, 0, copyFrom.Length);
        ResetDataUsed();
    }

    internal DigitsArray(DigitsArray copyFrom)
    {
        Allocate(copyFrom.Count, copyFrom.DataUsed);
        Array.Copy(copyFrom._data, 0, _data, 0, copyFrom.Count);
    }

   
    static DigitsArray()
    {
        unchecked
        {
            _allBits = (uint)~((uint)0);
            _hiBitSet = (uint)(((uint)1) << (DataSizeBits) - 1);
        }
    }

    public void Allocate(int size)
    {
        Allocate(size, 0);
    }

    public void Allocate(int size, int used)
    {
        _data = new uint[size + 1];
        _dataUsed = used;
    }

    internal void CopyFrom(uint[] source, int sourceOffset, int offset, int length)
    {
        Array.Copy(source, sourceOffset, _data, 0, length);
    }

    internal void CopyTo(uint[] array, int offset, int length)
    {
        Array.Copy(_data, 0, array, offset, length);
    }

    internal uint this[int index]
    {
        get
        {
            if (index < _dataUsed) return _data[index];
            return (IsNegative ? (uint)_allBits : (uint)0);
        }
        set { _data[index] = value; }
    }


    internal int Count
    {
        get { return _data.Length; }
    }

    internal bool IsZero
    {
        get { return _dataUsed == 0 || (_dataUsed == 1 && _data[0] == 0); }
    }

    internal bool IsNegative
    {
        get { return (_data[_data.Length - 1] & _hiBitSet) == _hiBitSet; }
    }

    internal void ResetDataUsed()
    {
        _dataUsed = _data.Length;
        if (IsNegative)
        {
            while (_dataUsed > 1 && _data[_dataUsed - 1] == _allBits)
            {
                --_dataUsed;
            }
            _dataUsed++;
        }
        else
        {
            while (_dataUsed > 1 && _data[_dataUsed - 1] == 0)
            {
                --_dataUsed;
            }
            if (_dataUsed == 0)
            {
                _dataUsed = 1;
            }
        }
    }

    internal int ShiftRight(int shiftCount)
    {
        return ShiftRight(_data, shiftCount);
    }

    internal static int ShiftRight(uint[] buffer, int shiftCount)
    {
        int shiftAmount = DataSizeBits;
        int invShift = 0;
        int bufLen = buffer.Length;

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
        {
            bufLen--;
        }

        for (int count = shiftCount; count > 0; count -= shiftAmount)
        {
            if (count < shiftAmount)
            {
                shiftAmount = count;
                invShift = DataSizeBits - shiftAmount;
            }

            ulong carry = 0;
            for (int i = bufLen - 1; i >= 0; i--)
            {
                ulong val = ((ulong)buffer[i]) >> shiftAmount;
                val |= carry;

                carry = ((ulong)buffer[i]) << invShift;
                buffer[i] = (uint)(val);
            }
        }

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
        {
            bufLen--;
        }

        return bufLen;
    }

    internal int ShiftLeft(int shiftCount)
    {
        return ShiftLeft(_data, shiftCount);
    }

    internal static int ShiftLeft(uint[] buffer, int shiftCount)
    {
        int shiftAmount = DataSizeBits;
        int bufLen = buffer.Length;

        while (bufLen > 1 && buffer[bufLen - 1] == 0)
        {
            bufLen--;
        }

        for (int count = shiftCount; count > 0; count -= shiftAmount)
        {
            if (count < shiftAmount)
            {
                shiftAmount = count;
            }

            ulong carry = 0;
            for (int i = 0; i < bufLen; i++)
            {
                ulong val = ((ulong)buffer[i]) << shiftAmount;
                val |= carry;

                buffer[i] = (uint)(val & _allBits);
                carry = (val >> DataSizeBits);
            }

            if (carry != 0)
            {
                if (bufLen + 1 <= buffer.Length)
                {
                    buffer[bufLen] = (uint)carry;
                    bufLen++;
                    carry = 0;
                }
                else
                {
                    throw new OverflowException();
                }
            }
        }
        return bufLen;
    }

    internal int ShiftLeftWithoutOverflow(int shiftCount)
    {
        List<uint> temp = new List<uint>(_data);
        int shiftAmount = DataSizeBits;

        for (int count = shiftCount; count > 0; count -= shiftAmount)
        {
            if (count < shiftAmount)
            {
                shiftAmount = count;
            }

            ulong carry = 0;
            for (int i = 0; i < temp.Count; i++)
            {
                ulong val = ((ulong)temp[i]) << shiftAmount;
                val |= carry;

                temp[i] = (uint)(val & _allBits);
                carry = (val >> DataSizeBits);
            }

            if (carry != 0)
            {
                temp.Add(0);
                temp[temp.Count - 1] = (uint)carry;
            }
        }
        _data = new uint[temp.Count];
        temp.CopyTo(_data);
        return _data.Length;
    }
}
*/