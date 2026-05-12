from fastapi import FastAPI

app = FastAPI()

# Yeh Tekla ka output hai (example data)
tekla_data = {
    "beam_id": "B-101",
    "profile": "IPE300",
    "material": "S235",
    "length": 4000
}

# AI check karega yeh data sahi hai ya nahi
@app.get("/check-beam")
def check_beam():
    errors = []

    # Check 1 - Profile sahi hai?
    valid_profiles = ["WRONG123", "HEA200", "HEA300"]
    if tekla_data["profile"] not in valid_profiles:
        errors.append("Wrong profile!")

    # Check 2 - Material sahi hai?
    valid_materials = ["S235", "S275", "S355"]
    if tekla_data["material"] not in valid_materials:
        errors.append("Wrong material!")

    # Check 3 - Length sahi hai?
    if tekla_data["length"] <= 0:
        errors.append("Wrong length!")

    if errors:
        return {"status": "ERROR", "errors": errors}
    else:
        return {"status": "OK", "message": "Beam data is correct!"}