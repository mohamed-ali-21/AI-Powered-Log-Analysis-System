import os
from dotenv import load_dotenv
from google.adk.agents import LlmAgent
from google.adk.models.lite_llm import LiteLlm

from .prompt import SYSTEM_PROMPT

load_dotenv()

assert os.getenv("GROQ_API_KEY"), "GROQ_API_KEY missing in .env"

_DEFAULT_MODEL = os.environ.get(
    "LOGMIND_MODEL",
    "groq/llama-3.3-70b-versatile"
)

root_agent = LlmAgent(
    name="logmind_analyst",
    description="Analyzes pre-grouped LogMind Issues and emits structured root-cause JSON.",
    model=LiteLlm(model=_DEFAULT_MODEL),
    instruction=SYSTEM_PROMPT,
    tools=[],
)