# Numerics

To help make converting numerical numbers into their character array counter part, a new module is introduced called 
`NumericUtils`.

Currently the only support are integers. Floating point will eventually be supported.

## How to use

To convert an integer to its character array counterpart, call the `ToCharArray(...)` extension function on a positive 
or negative integer value.

You can use the `stackalloc` keyword to temporarily create a buffer of characters onto the stack. For those unfamiliar 
with using the `stackalloc` keyword and its uses this will be a brief explanation.

Memory either lives in the stack, which is memory that follows a last in first out policy. This is temporary memory and 
is only scoped to its function call, so when a function needs to allocate a new integer it does so on the stack. 
While the function call will allocate a block of memory on top of the stack to execute, the moment its execution is 
finished, the allocated memory is released until it is used again. 

The heap is memory set aside for dynamic allocation. You can allocate a block of memory at any time and it generally 
does not follow the same structure as the stack. Heap memory is usually allocated at the start of the application, 
its runtime, and is released when the process exits.

`stackalloc` allows us to explicitly allocated a pointer on top of the stack and is released when the function leaves 
its scope.

Here is a good [article](https://vcsjones.dev/2020/02/24/stackalloc/) about the dos and don'ts of using `stackalloc`.

### Example
```cs
unsafe {
    // For positive values
    int value = 12345;

    char* positive = stackalloc char[5];
    value.ToCharArray(positive, 5, out int count); // This will fill the ptr with a max length of 5

    // For negative values

    value = -12345;
    char* negative = stackalloc char[6]; // We need 6 characters for the minus - character
    value.ToCharArray(negative, 6, out int count);
}
```
