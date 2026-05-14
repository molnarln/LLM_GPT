# DocGenerator
This is a university project for LLM_GPT course. This tool decorated a C# file's properties and methods with documentation strings using emdeddings and LLM models. The /test_data/testfile.cs is the file to be decorated. After successful run a new C# file will be generated in the test_data folder.

## Prerequisites
The project needs a running ollama service with "qwen2.5:1.5b" model loaded.

## Run the project
Start the virtual environment:
```
. venv/Scripts/activate
```
Install the requirements:
```
pip install -r requirements.txt
```
Build the CSDocsGenerator within it's project folder:
```
dotnet build
```
Start the project: 
```
python main.py
```
