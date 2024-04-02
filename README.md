# Unity to Godot Script Converter

This tool is designed to help you convert Unity scripts to Godot scripts, making it easier to migrate your game projects from Unity to Godot. It utilizes the Roslyn library for C# code analysis and transformation.

## Table of Contents

- [Getting Started](#getting-started)
- [Usage](#usage)
- [Examples](#examples)
- [Contributing](#contributing)
- [License](#license)

## Getting Started

### Prerequisites

- .NET SDK (7 or later) for building and running the project.
- A Godot project where you want to import the converted scripts.

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/a2937/unity-to-godot-converter.git
   ```

2. Navigate to the project folder:

```bash
  cd unity-to-godot-converter
```

3. Build the project:

```bash
  dotnet build
```

### Usage

1. Either build the build the application or download a binary
2. Run the converter (Assuming Windows is being used)

```bash
  .\UnityToGodotConverterConsole.exe
```

or

```bash
  .\UnityToGodotConverterConsole.exe <Path to your Unity C# file>
```

3. The converted file will be in the same place as the source file but end in an "\_Godot.cs" filename.

4. Manually fix any errors that occurred.

### Examples

- Converting Unity's `Start` method to Godot's `_Ready` method
- Replacing `GetComponent` calls with `GetNode` calls
- Debug statements

More features will be added as time goes on.

### Contributing

Contributions are welcome! Feel free to open issues, submit pull requests, or provide feedback.

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Special Thanks

DerrFunk for this [parser](https://gist.github.com/derFunk/795d7a366627d59e0dbd/revisions)
