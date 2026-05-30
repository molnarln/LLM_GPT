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


def get_context(new_code):
    """Megkeresi a leginkább hasonló mintát a DB-ben."""
    results = collection.query(
        query_texts=[new_code],
        n_results=1,
        include=["documents", "embeddings", "distances", "metadatas"],
    )
    print(f"A recall eredménye: {results['documents'][0][0]}\n")

    if results["metadatas"]:
        return results["metadatas"][0][0]["doc"]
    return ""


def validate_response(raw_content):
    if not raw_content:
        return ""

    lines = raw_content.split("\n")
    clean_lines = []

    for line in lines:
        stripped = line.strip()
        if not stripped:
            continue

        if not stripped.startswith("///"):
            print(
                f"   [VALIDÁCIÓS HIBA] Nem várt tartalom érkezett az LLM-től: '{stripped}'"
            )
            return ""
        clean_lines.append(line)
    return "\n".join(clean_lines)


def generate_documentation(new_code):
    # RAG
    sample_doc = get_context(new_code)

    # PROMPT
    prompt = f"""You are a C# documentation expert. 
Based on this sample style (use only the style, but NEVER use the parameters from the sample):
{sample_doc}

Generate XML documentation (summary, params, returns) for the following code.
Return ONLY the /// comments.

Code to document:
{new_code}
"""

    response = client.chat.completions.create(
        model="qwen2.5:1.5b",
        messages=[
            {
                "role": "system",
                "content": "You are a concise software engineering assistant.",
            },
            {"role": "user", "content": prompt},
        ],
        temperature=0.1,
    )

    raw_content = response.choices[0].message.content

    # Validálom, hogy valóban csak docstring jött vissza a modelltől
    return validate_response(raw_content)


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
