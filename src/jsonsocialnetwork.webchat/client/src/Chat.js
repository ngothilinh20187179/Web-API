import React from "react";
import { TextField, List, AppBar, Toolbar, IconButton, ListItem, Stack } from "@mui/material";
import axios from "axios";
import { HubConnectionBuilder } from "@microsoft/signalr";
import { Box } from "@mui/system";
import SearchIcon from '@mui/icons-material/Search';
import LogoutIcon from '@mui/icons-material/Logout';
import SendIcon from '@mui/icons-material/Send';

class Chat extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            partnerIdTextField: "",
            messageTextField: "",
            partnerId: null,
            messages: undefined,
            sendable: false
        };

        if (process.env.NODE_ENV === 'development') {
            this.connection = new HubConnectionBuilder()
                .withUrl("https://localhost:5001/messenger")
                .build();
        }
        else {
            this.connection = new HubConnectionBuilder()
                .withUrl("/messenger")
                .build();
        }
        this.connection.on(
            "Update",
            async (senderId) => {
                console.log("Update event invoked!");
                if (senderId === this.state.partnerId || senderId === this.props.user.id) {
                    var lastId;
                    if (this.state.messages.length === 0) {
                        lastId = 0;
                    }
                    else {
                        lastId = this.state.messages.at(-1).id;
                    }
                    const params = {
                        token: this.props.user.token,
                        partner_id: this.state.partnerId,
                        last_message_id : lastId
                    };
    
                    const res = await axios.post("/get_messages_from_last", null, { params });
                    switch (res.data.code) {
                        case "1000":
                            this.setState({ messages : [...this.state.messages, ...res.data.data] });
                            break;
                        case "9998":
                            console.log("Unauthorized token");
                            this.logoutBtn_onClick();
                            break;
                        case "9994":
                            console.log("Conversation doesn't exist");
                            break;
                        default:
                            console.log("API /get_messages_from_last ERROR");
                            break;
                    };
                }
            }
        );
    }


    async openConnection() {
        await this.connection.start();
        await this.connection.invoke('Initiate', this.props.user.id);
    }
    async closeConnection() {
        await this.connection.invoke("Close", this.props.user.id);
        await this.connection.stop();
    }


    textFieldChanged = (e) => this.setState({ [e.target.id]: e.target.value })

    findBtn_onClick = async () => {
        const params = {
            token: this.props.user.token,
            partnerAccountId: this.state.partnerIdTextField
        };
        if (!params.partnerAccountId) {
            console.log("Must type Partner ID");
            return;
        }
        if (params.partnerAccountId === this.props.user.id) {
            console.log("Not your ID, bruh!");
            return;
        }

        const res = await axios.post("/get_messages", null, { params });
        switch (res.data.code) {
            case "1000":
                this.setState({ sendable: true, messages: res.data.data, partnerId: this.state.partnerIdTextField });
                break;
            case "9994":
                console.log("Conversation doesn't exist yet");
                this.setState({ sendable: true, messages: [], partnerId: this.state.partnerIdTextField });
                break;
            case "9998":
            case "1002":
                console.log("Unauthorized token");
                this.logoutBtn_onClick();
                break;
            case "1004":
                console.log("Partner ID's wrong format");
                break;
            default:
                console.log("API /get_messages ERROR");
                break;
        }
    }

    logoutBtn_onClick = async () => {
        const params = { token: this.props.user.token };
        await axios.post("/logout", null, { params });
        this.props.setUser(undefined);
    }

    sendBtn_onClick = async () => {
        if (this.state.partnerId !== null) {
            const params = {
                token: this.props.user.token,
                partner_id: this.state.partnerId,
                body : this.state.messageTextField
            };
    
            const res = await axios.post("/set_message", null, { params });
            switch (res.data.code) {
                case "1000":
                    this.connection.invoke("Sent", this.props.user.id, this.state.partnerId);
                    this.setState({messageTextField: ""});
                    break;
                case "9998":
                    console.log("Unauthorized token!");
                    this.logoutBtn_onClick();
                    break;
                case "9995":
                    console.log("Partner ID is not exist");
                    break;
                default:
                    console.log("Something was wrong!");
                    break;
            };
            console.log("Send event complete!");
        }
    }



    componentDidMount() {
        this.openConnection();
    }

    componentWillUnmount() {
        this.closeConnection();
    }

    render() {
        return (
            <Box sx={{ bgcolor: "#0A1929" }}>
                <Stack>
                    <AppBar position="fixed"
                        sx={{ bgcolor: "#0B1D2F" }}>
                        <Toolbar>
                            <IconButton sx={{ ml: -1, mr: 2 }}
                                size="large"
                                onClick={this.logoutBtn_onClick}>
                                <LogoutIcon />
                            </IconButton>
                            <TextField id="partnerIdTextField"
                                sx={{ flexGrow: 1, mt: 1, mb: 1 }}
                                label="Partner ID"
                                type="number"
                                variant="outlined"
                                value={this.state.partnerIdTextField}
                                onChange={this.textFieldChanged}>
                            </TextField>
                            <IconButton sx={{ ml: 2, mr: -1 }}
                                size="large"
                                edge="start"
                                onClick={this.findBtn_onClick}>
                                <SearchIcon />
                            </IconButton>
                        </Toolbar>
                    </AppBar>

                    <List sx={{ marginTop: 8, marginBottom: 8 }}>
                        {this.state.messages !== undefined && this.state.messages.map(
                            ({ id, body, authorAccountId }) => {
                                if (authorAccountId === this.props.user.id) {
                                    return (
                                        <ListItem sx={{ justifyContent: "end" }} key={parseInt(id)}>
                                            <p style={{
                                                borderRadius: "25px",
                                                background: "#0084ff",
                                                color: "#ffffff",
                                                paddingTop: "10px",
                                                paddingBottom: "10px",
                                                paddingLeft: "20px",
                                                paddingRight: "20px",
                                                minWidth: "20px",
                                                minHeight: "20px",
                                                margin: "0px",
                                                textAlign: "end"
                                            }}>
                                                {body}
                                            </p>
                                        </ListItem>
                                    );
                                } else {
                                    return (
                                        <ListItem key={parseInt(id)}>
                                            <p style={{
                                                borderRadius: "25px",
                                                background: "#3e4042",
                                                color: "#ffffff",
                                                paddingTop: "10px",
                                                paddingBottom: "10px",
                                                paddingLeft: "20px",
                                                paddingRight: "20px",
                                                minWidth: "20px",
                                                minHeight: "20px",
                                                margin: "0px"
                                            }}>
                                                {body}
                                            </p>
                                        </ListItem>
                                    );
                                }
                            }
                        )}
                    </List>

                    <AppBar position="fixed"
                        sx={{ bgcolor: "#0B1D2F", top: "auto", bottom: 0 }}>
                        <Toolbar>
                            <TextField id="messageTextField"
                                sx={{ flexGrow: 1, mt: 1, mb: 1 }}
                                label="Message"
                                variant="outlined"
                                value={this.state.messageTextField}
                                onChange={this.textFieldChanged}>
                            </TextField>
                            <IconButton sx={{ ml: 2, mr: -1 }}
                                size="large"
                                disabled={!this.state.sendable}
                                onClick={this.sendBtn_onClick}>
                                <SendIcon />
                            </IconButton>
                        </Toolbar>
                    </AppBar>
                </Stack>
            </Box>
        );
    }
}

export default Chat;