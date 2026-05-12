from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

class UserRequest(BaseModel):
    message: str

@app.post("/create-structure")
def create_structure(request: UserRequest):
    generated_code = """
using Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;

Model model = new Model();
if (model.GetConnectionStatus())
{
    Beam beam = new Beam();
    beam.StartPoint = new Point(0, 0, 0);
    beam.EndPoint = new Point(4000, 0, 0);
    beam.Profile.ProfileString = "IPE300";
    beam.Material.MaterialString = "S275";
    beam.Insert();
    model.CommitChanges();
}
"""
    return {
        "user_request": request.message,
        "generated_code": generated_code
    }

@app.get("/")
def home():
    return {"message": "Tekla FastAPI is running!"}