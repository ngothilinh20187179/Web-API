import React from "react";
import { Button, Card, TextField, Grid, Alert, Snackbar } from "@mui/material";
import axios from "axios";

class LoginForm extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            phone: "",
            pass: "",
            isAlertOpen: false,
            alertMessage: ""
        };
    }

    textFieldChanged = (e) => this.setState({ [e.target.id]: e.target.value })
    openAlert(message) {
        this.setState({ isAlertOpen: true, alertMessage: message });
    }

    loginBtn_onClick = async () => {
        const params = {
            phonenumber: this.state.phone,
            password: this.state.pass,
        };
        if (!(params.phonenumber && params.password)) {
            this.openAlert("Please type phone and password!");
            return;
        }

        const res = await axios.post("/login", null, { params });
        switch (res.data.code) {
            case "1000":
                console.log(res.data.data);
                this.props.setUser(res.data.data);
                break;
            case "1004":
                this.openAlert("Either phone or password was wrong format!");
                break;
            case "9995":
                this.openAlert("Phone does not exist!");
                break;
            case "9993":
                this.openAlert("Wrong password!");
                break;
            default:
                this.openAlert("Something was wrong!");
                break;
        }
    }


    render() {
        return (
            <Grid
                sx={{ minHeight: "100vh", bgcolor: "#0A1929" }}
                container
                direction="column"
                alignItems="center"
                justifyContent="center">
                <Card
                    sx={{
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "stretch",
                        justifyContent: "space-evenly",
                        p: 4,
                        width: 300,
                        bgcolor: "#0D2238",
                        borderRadius: 2,
                    }}>
                    <TextField
                        sx={{ m: 1 }}
                        id="phone"
                        label="Phone"
                        type="tel"
                        variant="outlined"
                        value={this.state.phone}
                        onChange={this.textFieldChanged} />
                    <TextField
                        sx={{ m: 1 }}
                        id="pass"
                        label="Password"
                        type="password"
                        variant="outlined"
                        value={this.state.pass}
                        onChange={this.textFieldChanged} />
                    <Button
                        sx={{ m: 1 }}
                        variant="contained"
                        onClick={this.loginBtn_onClick}>
                        Login
                    </Button>
                </Card>

                <Snackbar
                    open={this.state.isAlertOpen}
                    autoHideDuration={3000}
                    onClose={() => this.setState({ isAlertOpen: false })}>
                    <Alert severity="error">
                        {this.state.alertMessage}
                    </Alert>
                </Snackbar>
            </Grid>
        );
    }
}

export default LoginForm;