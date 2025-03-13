# Mini-Compiler

A simple yet powerful compiler for a custom C-like programming language, built using ANTLR. This project allows parsing and compiling of a subset of C-style syntax, including variable declarations, expressions, loops, functions, and control structures. This project was made with [Moroianu Andrei](https://github.com/MoroianuAndrei/Compiler.git).

# 📌Features
  ✅ Custom C-like Syntax: Supports `int`, `float`, `double`, `string`, and `void` types.
  
  ✅ Control Flow: Implements `if`, `else`, `for`, and `while` statements.
  
  ✅ Operators: Supports arithmetic (`+`, `-`, `*`, `/`, `%`), logical (`&&`, `||`, `!`), and comparison (`==`, `!=`, `<`, `>`, `<=`, `>=`) operators.
  
  ✅ Function Support: Allows function definitions, calls, and return values.
  
  ✅ Increment & Decrement Handling: Parses `++i`, `i++`, `--i`, and `i--` correctly.
  
  ✅ Lexer & Parser via ANTLR: Implements a complete ANTLR-based frontend.
  
  ✅ Error Handling: Provides syntax error reporting with precise line/column information.

# 🚀 Getting Started

🔧 Prerequisites
Ensure you have the following installed:

  * Java (JDK 8+ recommended)
  * ANTLR 4 (Install via `pip install antlr4-tools` or manually)
  * A build system (`Gradle` or `Maven` recommended)
📥 Installation

  1. Clone the repository
  2. Generate the ANTLR parser and lexer: `antlr4 -Dlanguage=Java src/grammar/BasicLanguage.g4 -o src/parser`
  3. Compile the project: `javac -cp .:antlr-4.x-complete.jar src/compiler/*.java`
     
▶️ Running the Compiler

To compile a BasicLanguage script:

`java -cp .:antlr-4.x-complete.jar compiler.Compiler examples/sample.bl`

Replace `sample.bl` with your script written in BasicLanguage.

# 🏗️ Architecture

This compiler consists of multiple stages:

1️⃣ Lexical Analysis (Lexer)

  * Converts raw text into tokens (`int`, `for`, `+`, `==`, etc.).
2️⃣ Parsing (Parser)

  * Uses ANTLR to construct a parse tree based on syntax rules.

# ❌ Error Handling

The compiler currently detects syntax errors and reports:

  * Unexpected tokens (e.g., `int = 5;` instead of `int x = 5;`)
  * Mismatched parentheses
  * Incorrect loop syntax
# 📊 Roadmap

  * Basic grammar implementation
  * `for`, `while`, `if-else` support
  * Function definitions & calls

# 🧪 Testing

Run unit tests to validate the compiler:

`java -cp .:antlr-4.x-complete.jar org.junit.runner.JUnitCore TestSuite`

Testing includes:

  ✅ Lexer token recognition
  
  ✅ Parsing of valid programs
  
  ✅ Detection of syntax errors

