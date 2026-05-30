import json
import subprocess
import chromadb
from openai import OpenAI

client = OpenAI(base_url="http://localhost:11434/v1", api_key="ollama")
chroma_client = chromadb.PersistentClient(path="./docs_db")
collection = chroma_client.get_or_create_collection(name="csharp_docs")

EMBEDDING_SEED_JSON = "C:\\Users\\laszl\\SourceCodes\\LLM_GPT\\embedding_seed.json"
TESTFILE = "C:\\Users\\laszl\\SourceCodes\\LLM_GPT\\test_data\\testfile.cs"
PARSER = "C:\\Users\\laszl\\SourceCodes\\LLM_GPT\\CSDocsGenerator\\CSFileParser\\bin\\Debug\\CSFileParser.exe"
AI_GENERATED_DOCS = (
    "C:\\Users\\laszl\\SourceCodes\\LLM_GPT\\test_data\\ai_generated_docs.json"
)
EXTRACTED_MEMBERS_JSON = (
    "C:\\Users\\laszl\\SourceCodes\\LLM_GPT\\test_data\\extracted_members.json"
)


def run_roslyn_parser(file_path):
    subprocess.run([PARSER, "p", file_path])
    print(f"Roslyn elemzi a fájlt: {file_path}")

    with open(EXTRACTED_MEMBERS_JSON, "r") as f:
        return json.load(f)


def seed_database(json_path):
    with open(json_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    if collection.count() == 0:
        collection.add(
            documents=[item["code"] for item in data],
            metadatas=[{"doc": item["doc"], "cat": item["category"]} for item in data],
            ids=[item["id"] for item in data],
        )
        print(f"Adatbázis feltöltve: {collection.count()} minta.")


def fetch_rag_style_sample(csharp_code):
    print(f"-> [TOOL FUTÁS] ChromaDB lekérdezése ehhez a kódhoz...")
    results = collection.query(
        query_texts=[csharp_code],
        n_results=1,
        include=["documents", "embeddings", "distances", "metadatas"],
    )

    if results["metadatas"] and results["metadatas"][0]:
        sample = results["metadatas"][0][0]["doc"]
        print(f"-> [TOOL SIKER] Talált stílusminta mérete: {len(sample)} karakter.")
        return sample
    return "Nincs releváns minta az adatbázisban. Használj általános, professzionális C# XML stílust."


rag_tool_definition = {
    "type": "function",
    "function": {
        "name": "fetch_rag_style_sample",
        "description": "Accesses the local ChromaDB vector database and retrieves a similar XML documentation style sample (evidence) for the given code.",
        "parameters": {
            "type": "object",
            "properties": {
                "csharp_code": {
                    "type": "string",
                    "description": "The C# source code of the method or property to find a matching style sample for.",
                }
            },
            "required": ["csharp_code"],
        },
    },
}


def generate_documentation(new_code):
    messages = [
        {
            "role": "system",
            "content": (
                "You are an advanced C# development assistant. "
                "CRITICAL INSTRUCTION: You do not know the codebase's documentation style yet. "
                "Therefore, before generating ANY documentation, you MUST call the 'fetch_rag_style_sample' tool "
                "to retrieve the matching style template from the ChromaDB database. "
                "Do not attempt to write the documentation from your own memory first. Call the tool immediately."
            ),
        },
        {
            "role": "user",
            "content": (
                f"I need you to generate C# XML documentation (/// comments) for the target code below. "
                f"But FIRST, you must call the 'fetch_rag_style_sample' tool using this target code as the argument to find the style template:\n\n"
                f"{new_code}"
            ),
        },
    ]

    response = client.chat.completions.create(
        model="qwen2.5:7b",
        messages=messages,
        tools=[rag_tool_definition],
        temperature=0.1,
    )

    response_message = response.choices[0].message

    if response_message.tool_calls:
        print("   [LLM SIKER] A 7B modell Tool-t hívott!")
        for tool_call in response_message.tool_calls:
            function_name = tool_call.function.name

            if function_name == "fetch_rag_style_sample":
                tool_args = json.loads(tool_call.function.arguments)

                raw_tool_output = fetch_rag_style_sample(
                    csharp_code=tool_args.get("csharp_code")
                )

                lines = raw_tool_output.split("\n")
                only_comments = [
                    line.strip() for line in lines if line.strip().startswith("///")
                ]
                clean_style_sample = "\n".join(only_comments)

                if not clean_style_sample:
                    clean_style_sample = (
                        "/// <summary>\n/// Standard documentation.\n/// </summary>"
                    )

                messages.append(response_message)
                messages.append(
                    {
                        "role": "tool",
                        "tool_call_id": tool_call.id,
                        "name": function_name,
                        "content": (
                            f"Retrieved Formatting Style (Use ONLY this formatting template, "
                            f"IGNORE any parameter names or values from it):\n{clean_style_sample}"
                        ),
                    }
                )

                messages.append(
                    {
                        "role": "user",
                        "content": "Now, write ONLY the /// comments for the target code. Do not write markdown blocks, do not write the function definition.",
                    }
                )

                final_response = client.chat.completions.create(
                    model="qwen2.5:7b",
                    messages=messages,
                    temperature=0.0,
                )

                final_text = final_response.choices[0].message.content
                final_lines = final_text.split("\n")
                filtered_lines = [l for l in final_lines if l.strip().startswith("///")]

                return "\n".join(filtered_lines)

    print("   [LLM FALLBACK] Nem hívott tool-t.")
    return response_message.content


if __name__ == "__main__":
    seed_database(EMBEDDING_SEED_JSON)

    members = run_roslyn_parser(TESTFILE)

    print(f"\nTalált elemek száma: {len(members)}")

    results = {}

    for member in members:
        print(f"\n--- Dokumentálás: {member['name']} ({member['type']}) ---")
        doc = generate_documentation(member["fullCode"])
        results[member["name"]] = doc

        with open(AI_GENERATED_DOCS, "w", encoding="utf-8") as f:
            json.dump(results, f, ensure_ascii=False, indent=2)

        subprocess.run([PARSER, "w", AI_GENERATED_DOCS, TESTFILE])

        print(doc)
