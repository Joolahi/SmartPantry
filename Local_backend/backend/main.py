from fastapi import FastAPI
from services.auth_services import register_user
from services.login_services import login_user
from schemas import RegisterRequest, LoginRequest


app = FastAPI()
@app.get("/")
def read_root():
    return {"Hello": "World"}

@app.post("/register")
def register(req: RegisterRequest):
    return register_user(req)

@app.post("/login")
def login(req: LoginRequest):
    return login_user(req)