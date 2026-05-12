from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

# Yeh Tekla model ka extracted data hai
tekla_model_data = {
    "elements": [
        {
            "beam_id": "B-101",
            "type": "beam",
            "profile": "IPE300",
            "material": "S275",
            "start_point": {"x": 0, "y": 0, "z": 0},
            "end_point": {"x": 4000, "y": 0, "z": 0},
            "length": 4000
        },
        {
            "beam_id": "C-101",
            "type": "column",
            "profile": "HEA200",
            "material": "S275",
            "start_point": {"x": 0, "y": 0, "z": 0},
            "end_point": {"x": 0, "y": 0, "z": 3500},
            "length": 3500
        }
    ]
}

# Tekla se data extract karo
@app.get("/extract-data")
def extract_data():
    return {
        "status": "success",
        "message": "Data extracted from Tekla model",
        "data": tekla_model_data
    }

# HITL - Human check karega data
@app.get("/validate-data")
def validate_data():
    errors = []
    for element in tekla_model_data["elements"]:
        if element["length"] <= 0:
            errors.append(f"{element['beam_id']} - Wrong length!")
        if element["profile"] not in ["IPE300", "HEA200", "HEA300"]:
            errors.append(f"{element['beam_id']} - Wrong profile!")

    if errors:
        return {"status": "ERROR", "errors": errors}
    else:
        return {"status": "OK", "message": "All elements are valid!"}