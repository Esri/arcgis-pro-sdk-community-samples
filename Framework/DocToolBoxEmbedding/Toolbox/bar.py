import os 
 
def hello(): 
    return f'Hello {os.getenv("username")}'
