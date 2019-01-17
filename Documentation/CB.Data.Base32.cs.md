# CB.Data.Base32.cs

## class `ClearBase32`

ClearBase32 is similar to the classic Base64 encoding, except:
- It is limited to 32 characters only.
- The character set does not include characters that could be confounded. Such as 'I' and 'l', 'I' and '1', 'l' and '1', 'O' and '0'.
- There is not markup for beginning, ending or hashing. All the text is the code.

Such code is intended to be typed manually by a user. That is why it has to be clear and simple.

You can encode from a byte array or a string, to a base32 string.
Then decode the base32 string to either a byte array or a string.

## Main functions

- byte[] → base32 (string)  
`string Encode(byte[] source)`  
Encodes a byte array to a base32 string.

- base32 (string) → byte[]  
`byte[] Decode(string source)`  
Decodes a base32 string to a byte array, retreiving the original data.

## When the source is (itself) a string

- string → base32 (string)  
`string EncodeText(string source)`  
Encodes a regular string to a base32 string.

- base32 (string) → string  
`string DecodeAsText(string source)`  
Decodes a base32 string to a regular string, retreiving the original data.

## Storing the coded base32 in a byte array

It can be useful when serializing the base32, for example.

### The original data is a byte array

- byte[] → base32 (byte[])  
`byte[] EncodeToAByteArray(byte[] source)`  
Encodes a byte array to a base32 string.

- base32 (byte[]) → byte[]  
`byte[] DecodeFromByteArray(byte[] source)`  
Decodes a base32 string to a byte array, retreiving the original data.

### The original data is a regular string

- string → base32 (byte[])  
`byte[] EncodeTextToAByteArray(string source)`  
Encodes a byte array to a base32 string.

- base32 (byte[]) → string  
`string DecodeAsStringFromByteArray(byte[] source)`  
Decodes a base32 string to a byte array, retreiving the original data.
---

[Go back to the table of contents](../readme.md)

---
Copyright (c) [Christophe Bertrand](https://chrisbertrand.net)  
https://github.com/ChrisBertrandDotNet/CB-Helpers
