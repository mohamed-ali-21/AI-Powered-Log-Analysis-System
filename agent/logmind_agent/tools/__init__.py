# Future ADK tools live here. Register them in ../agent.py via:
#
#     from google.adk.tools import FunctionTool
#     from .tools.service_health import fetch_service_health
#
#     tools=[FunctionTool(fetch_service_health), ...]
#
# Each tool is a plain Python callable with type annotations and a docstring.
# ADK auto-generates the function-calling schema from those.
